// bridge.js — Tauri ↔ Frontend bridge
// Detects Tauri environment and provides data access.
// Falls back to hardcoded data when running in plain browser.

const IS_TAURI = Boolean(window.__TAURI__);

const bridge = {
  _wordData: [],
  _phases: null,
  _listeners: [],

  async init() {
    if (IS_TAURI) {
      // Load data from Rust backend
      const { invoke } = window.__TAURI__.core;
      const { listen } = window.__TAURI__.event;

      // Initial data load
      this._wordData = await invoke('get_word_data');
      this._phases = await invoke('get_phases');

      // Set globals for components that read window.WORD_DATA
      window.WORD_DATA = this._wordData;

      // Listen for live updates from file watcher
      listen('word-data-updated', (event) => {
        this._wordData = event.payload;
        window.WORD_DATA = event.payload;
        this._notify('word-data-updated', event.payload);
        console.log('[bridge] Word data updated from file watcher');
      });

      console.log(`[bridge] Tauri mode. ${this._wordData.length} words loaded.`);
    } else {
      // Browser fallback — wait for WORD_DATA to be defined
      console.log('[bridge] Browser mode. Using embedded data.');
      await this._waitForWordData();
    }
  },

  async _waitForWordData() {
    return new Promise((resolve) => {
      const check = () => {
        if (window.WORD_DATA && window.WORD_DATA.length > 0) {
          this._wordData = window.WORD_DATA;
          resolve();
        } else {
          setTimeout(check, 100);
        }
      };
      check();
    });
  },

  getWordData() {
    return this._wordData;
  },

  getPhases() {
    return this._phases;
  },

  async toggleGate(phaseId, gateIdx, satisfied) {
    if (IS_TAURI) {
      const { invoke } = window.__TAURI__.core;
      await invoke('toggle_gate', { phaseId, gateIdx, satisfied });
    } else {
      // localStorage fallback
      const key = `shannon-gate-${phaseId}-${gateIdx}`;
      localStorage.setItem(key, satisfied ? '1' : '0');
    }
  },

  async setWordStatus(word, status) {
    if (IS_TAURI) {
      const { invoke } = window.__TAURI__.core;
      await invoke('set_word_status', { word, status });
    }
  },

  async runTests() {
    if (IS_TAURI) {
      const { invoke } = window.__TAURI__.core;
      return await invoke('run_tests');
    }
    return null;
  },

  async getTestResults() {
    if (IS_TAURI) {
      const { invoke } = window.__TAURI__.core;
      return await invoke('get_test_results');
    }
    return null;
  },

  async getTestHistory() {
    if (IS_TAURI) {
      const { invoke } = window.__TAURI__.core;
      return await invoke('get_test_history');
    }
    return [];
  },

  // Event subscription for components
  on(event, callback) {
    this._listeners.push({ event, callback });
  },

  _notify(event, data) {
    for (const l of this._listeners) {
      if (l.event === event) {
        try { l.callback(data); } catch (e) { console.error(e); }
      }
    }
  }
};

// Auto-init
bridge.init();

// Export globally
window.bridge = bridge;
