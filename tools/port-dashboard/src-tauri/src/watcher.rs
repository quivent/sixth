use notify::{Config, Event, RecommendedWatcher, RecursiveMode, Watcher};
use std::path::PathBuf;
use std::sync::mpsc;
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager};

use crate::state::AppState;

/// Start the file watcher on the compiler/shannon/ directory.
/// Debounces events by 500ms, then triggers a rescan and emits updates.
pub fn start_watcher(app: AppHandle, project_root: PathBuf) -> Result<(), String> {
    let watch_dir = project_root.join("compiler/shannon");
    if !watch_dir.exists() {
        return Err(format!("Watch directory not found: {}", watch_dir.display()));
    }

    std::thread::spawn(move || {
        let (tx, rx) = mpsc::channel::<Event>();

        let mut watcher = RecommendedWatcher::new(
            move |res: Result<Event, notify::Error>| {
                if let Ok(event) = res {
                    let _ = tx.send(event);
                }
            },
            Config::default(),
        )
        .expect("Failed to create file watcher");

        watcher
            .watch(&watch_dir, RecursiveMode::Recursive)
            .expect("Failed to watch directory");

        eprintln!("[watcher] Watching: {}", watch_dir.display());

        let debounce = Duration::from_millis(500);
        let mut last_event = Instant::now() - debounce;

        loop {
            match rx.recv_timeout(Duration::from_secs(1)) {
                Ok(event) => {
                    // Only care about .fs file changes
                    let is_fs = event.paths.iter().any(|p| {
                        p.extension().map_or(false, |e| e == "fs")
                    });

                    if !is_fs {
                        continue;
                    }

                    let now = Instant::now();
                    if now.duration_since(last_event) < debounce {
                        continue;
                    }
                    last_event = now;

                    eprintln!(
                        "[watcher] Change detected: {:?}",
                        event.paths.iter().map(|p| p.file_name().unwrap_or_default()).collect::<Vec<_>>()
                    );

                    // Trigger rescan
                    if let Some(state) = app.try_state::<AppState>() {
                        state.rescan();
                        // Get updated word data
                        let words = state.get_word_data();
                        let _ = app.emit("word-data-updated", &words);
                        eprintln!("[watcher] Emitted word-data-updated ({} words)", words.len());
                    }
                }
                Err(mpsc::RecvTimeoutError::Timeout) => continue,
                Err(mpsc::RecvTimeoutError::Disconnected) => {
                    eprintln!("[watcher] Channel disconnected, stopping");
                    break;
                }
            }
        }
    });

    Ok(())
}
