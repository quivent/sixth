#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod benchmark;
mod commands;
mod db;
mod models;
mod parser;
mod runner;
mod state;
mod watcher;

use std::path::PathBuf;

use state::AppState;

fn find_project_root(start: &std::path::Path) -> Option<PathBuf> {
    let mut current = start.to_path_buf();
    loop {
        // Check if compiler/shannon/ exists here
        if current.join("compiler/shannon").exists() {
            return Some(current);
        }
        // Also check tools/port-dashboard as a marker
        if current.join("tools/port-dashboard").exists() && current.join("compiler").exists() {
            return Some(current);
        }
        // Move up one level
        if !current.pop() {
            return None;
        }
    }
}

fn main() {
    // Resolve project root by looking for compiler/shannon/ directory
    // Start from CWD and walk up until we find it
    let cwd = std::env::current_dir().unwrap();
    let project_root = find_project_root(&cwd).unwrap_or_else(|| {
        // Fallback: try env var or current dir
        std::env::var("SIXTH_ROOT")
            .map(PathBuf::from)
            .unwrap_or_else(|_| cwd.clone())
    });

    eprintln!("[dashboard] Project root: {}", project_root.display());

    let dashboard_dir = project_root.join("tools/port-dashboard");
    let db_path = dashboard_dir.join("port-dashboard.db");
    let manifest_path = dashboard_dir.join("word-manifest.json");

    let app_state = AppState::new(project_root.clone(), &db_path, &manifest_path);

    tauri::Builder::default()
        .manage(app_state)
        .invoke_handler(tauri::generate_handler![
            commands::get_word_data,
            commands::get_phases,
            commands::get_test_results,
            commands::run_tests,
            commands::refresh_adversarial_tests,
            commands::get_adversarial_summary,
            commands::toggle_gate,
            commands::set_word_status,
            commands::get_test_history,
            commands::get_project_root,
            commands::read_source_file,
            commands::get_file_inventory,
            commands::scan_all_tests,
            commands::run_all_tests,
            commands::read_test_file,
            commands::run_interactive_test,
            commands::get_arm64_status,
            commands::get_arm64_test_results,
            commands::run_arm64_tests,
            commands::get_benchmarks,
            commands::run_benchmarks,
            commands::run_benchmark,
        ])
        .setup(move |app| {
            let handle = app.handle().clone();
            // Start file watcher
            if let Err(e) = watcher::start_watcher(handle, project_root) {
                eprintln!("[dashboard] Watcher failed: {}", e);
            }
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
