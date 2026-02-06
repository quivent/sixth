use rusqlite::{Connection, params};
use std::collections::HashMap;
use std::path::Path;

use crate::models::{TestRun, WordStatus};

pub struct Database {
    conn: Connection,
}

impl Database {
    pub fn open(path: &Path) -> rusqlite::Result<Self> {
        let conn = Connection::open(path)?;

        conn.execute_batch(
            "CREATE TABLE IF NOT EXISTS gate_checks (
                phase_id INTEGER NOT NULL,
                gate_idx INTEGER NOT NULL,
                satisfied INTEGER NOT NULL DEFAULT 0,
                updated_at TEXT NOT NULL DEFAULT (datetime('now')),
                PRIMARY KEY (phase_id, gate_idx)
            );

            CREATE TABLE IF NOT EXISTS test_runs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                run_at TEXT NOT NULL DEFAULT (datetime('now')),
                total INTEGER NOT NULL DEFAULT 0,
                pass INTEGER NOT NULL DEFAULT 0,
                wrong INTEGER NOT NULL DEFAULT 0,
                cfail INTEGER NOT NULL DEFAULT 0,
                rfail INTEGER NOT NULL DEFAULT 0,
                wall_ms INTEGER NOT NULL DEFAULT 0,
                compile_ms INTEGER NOT NULL DEFAULT 0,
                run_ms INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS word_overrides (
                word TEXT PRIMARY KEY,
                status TEXT NOT NULL,
                updated_at TEXT NOT NULL DEFAULT (datetime('now'))
            );"
        )?;

        Ok(Database { conn })
    }

    // === Gate checks ===

    pub fn get_gate(&self, phase_id: u8, gate_idx: u32) -> bool {
        self.conn
            .query_row(
                "SELECT satisfied FROM gate_checks WHERE phase_id = ?1 AND gate_idx = ?2",
                params![phase_id, gate_idx],
                |row| row.get::<_, bool>(0),
            )
            .unwrap_or(false)
    }

    pub fn set_gate(&self, phase_id: u8, gate_idx: u32, satisfied: bool) -> rusqlite::Result<()> {
        self.conn.execute(
            "INSERT INTO gate_checks (phase_id, gate_idx, satisfied, updated_at)
             VALUES (?1, ?2, ?3, datetime('now'))
             ON CONFLICT(phase_id, gate_idx) DO UPDATE SET
               satisfied = excluded.satisfied,
               updated_at = excluded.updated_at",
            params![phase_id, gate_idx, satisfied],
        )?;
        Ok(())
    }

    pub fn get_all_gates(&self) -> HashMap<(u8, u32), bool> {
        let mut stmt = self
            .conn
            .prepare("SELECT phase_id, gate_idx, satisfied FROM gate_checks")
            .unwrap();
        let mut map = HashMap::new();
        let rows = stmt
            .query_map([], |row| {
                Ok((
                    row.get::<_, u8>(0)?,
                    row.get::<_, u32>(1)?,
                    row.get::<_, bool>(2)?,
                ))
            })
            .unwrap();
        for row in rows.flatten() {
            map.insert((row.0, row.1), row.2);
        }
        map
    }

    // === Word overrides ===

    pub fn get_word_overrides(&self) -> HashMap<String, WordStatus> {
        let mut stmt = self
            .conn
            .prepare("SELECT word, status FROM word_overrides")
            .unwrap();
        let mut map = HashMap::new();
        let rows = stmt
            .query_map([], |row| {
                Ok((row.get::<_, String>(0)?, row.get::<_, String>(1)?))
            })
            .unwrap();
        for row in rows.flatten() {
            map.insert(row.0, WordStatus::from_str(&row.1));
        }
        map
    }

    pub fn set_word_override(&self, word: &str, status: &WordStatus) -> rusqlite::Result<()> {
        self.conn.execute(
            "INSERT INTO word_overrides (word, status, updated_at)
             VALUES (?1, ?2, datetime('now'))
             ON CONFLICT(word) DO UPDATE SET
               status = excluded.status,
               updated_at = excluded.updated_at",
            params![word, status.as_str()],
        )?;
        Ok(())
    }

    pub fn remove_word_override(&self, word: &str) -> rusqlite::Result<()> {
        self.conn
            .execute("DELETE FROM word_overrides WHERE word = ?1", params![word])?;
        Ok(())
    }

    // === Test runs ===

    pub fn save_test_run(&self, run: &crate::models::TestSummary) -> rusqlite::Result<()> {
        self.conn.execute(
            "INSERT INTO test_runs (total, pass, wrong, cfail, rfail, wall_ms, compile_ms, run_ms)
             VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8)",
            params![
                run.total,
                run.pass,
                run.wrong,
                run.cfail,
                run.rfail,
                run.wall_ms,
                run.compile_ms,
                run.run_ms
            ],
        )?;
        Ok(())
    }

    pub fn get_test_history(&self, limit: u32) -> Vec<TestRun> {
        let mut stmt = self
            .conn
            .prepare(
                "SELECT id, run_at, total, pass, wrong, cfail, rfail, wall_ms, compile_ms, run_ms
                 FROM test_runs ORDER BY id DESC LIMIT ?1",
            )
            .unwrap();
        let rows = stmt
            .query_map(params![limit], |row| {
                Ok(TestRun {
                    id: row.get(0)?,
                    run_at: row.get(1)?,
                    total: row.get(2)?,
                    pass: row.get(3)?,
                    wrong: row.get(4)?,
                    cfail: row.get(5)?,
                    rfail: row.get(6)?,
                    wall_ms: row.get(7)?,
                    compile_ms: row.get(8)?,
                    run_ms: row.get(9)?,
                })
            })
            .unwrap();
        rows.flatten().collect()
    }
}
