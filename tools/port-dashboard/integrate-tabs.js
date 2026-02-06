#!/usr/bin/env node
// Integrate Tracker, Vocabulary, and Optimizations tabs into index.html
// Also adds a code viewer modal

const fs = require('fs');
const path = require('path');

const indexPath = path.join(__dirname, 'src/index.html');
const trackerPath = path.join(__dirname, 'src/tracker-tab.html');
const vocabPath = path.join(__dirname, 'src/vocab-tab.html');
const optPath = path.join(__dirname, 'src/opt-tab.html');

let html = fs.readFileSync(indexPath, 'utf8');
const trackerHtml = fs.readFileSync(trackerPath, 'utf8');
const vocabHtml = fs.readFileSync(vocabPath, 'utf8');
const optHtml = fs.readFileSync(optPath, 'utf8');

const lines = html.split('\n');

// Step 1: Add tab buttons (find the </div> after the last tab)
// Look for the line with data-panel="audit-view">Audit</div>
let tabInsertIdx = -1;
for (let i = 0; i < lines.length; i++) {
  if (lines[i].includes('data-panel="audit-view"')) {
    tabInsertIdx = i + 1;
    break;
  }
}

if (tabInsertIdx === -1) {
  console.error('Could not find audit tab button');
  process.exit(1);
}

// Check if tabs already exist
if (html.includes('data-panel="tracker-view"')) {
  console.log('Tabs already integrated, skipping tab button insertion.');
} else {
  const newTabs = [
    '  <div class="tab" data-panel="tracker-view">Tracker</div>',
    '  <div class="tab" data-panel="vocab-view">Vocabulary</div>',
    '  <div class="tab" data-panel="opt-view">Optimizations</div>',
  ];
  lines.splice(tabInsertIdx, 0, ...newTabs);
  console.log(`Inserted 3 tab buttons at line ${tabInsertIdx + 1}`);
}

// Step 2: Find the TAB SWITCHING comment and insert panels before it
html = lines.join('\n');
const tabSwitchComment = '<!-- TAB SWITCHING';
const tabSwitchIdx = html.indexOf(tabSwitchComment);
if (tabSwitchIdx === -1) {
  console.error('Could not find TAB SWITCHING comment');
  process.exit(1);
}

// Check if panels already exist
if (html.includes('id="tracker-view"') && html.includes('id="vocab-view"') && html.includes('id="opt-view"')) {
  console.log('Panels already integrated, skipping panel insertion.');
} else {
  // Insert all three panels before TAB SWITCHING
  const panelBlock = '\n\n' + trackerHtml + '\n\n' + vocabHtml + '\n\n' + optHtml + '\n\n';
  html = html.slice(0, tabSwitchIdx) + panelBlock + html.slice(tabSwitchIdx);
  console.log('Inserted Tracker, Vocabulary, and Optimizations panels');
}

// Step 3: Add code viewer modal (before closing </body>)
if (!html.includes('id="code-viewer-modal"')) {
  const codeViewerModal = `
<!-- ============================================================ -->
<!-- CODE VIEWER MODAL                                             -->
<!-- ============================================================ -->
<div id="code-viewer-modal" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.85); z-index:10000; overflow:auto;">
  <div style="max-width:900px; margin:2rem auto; background:#0d1117; border:1px solid #333; border-radius:8px; overflow:hidden;">
    <div style="display:flex; justify-content:space-between; align-items:center; padding:0.75rem 1rem; background:#161b22; border-bottom:1px solid #333;">
      <span id="code-viewer-title" style="font-family:monospace; color:#e0e0e0; font-size:0.9rem;"></span>
      <button onclick="closeCodeViewer()" style="background:none; border:1px solid #555; color:#999; padding:4px 12px; border-radius:4px; cursor:pointer; font-size:0.85rem;">&times; Close</button>
    </div>
    <pre id="code-viewer-content" style="padding:1rem; margin:0; overflow-x:auto; font-size:0.82rem; line-height:1.5; color:#c9d1d9; background:#0d1117;"><code>Loading...</code></pre>
  </div>
</div>
<script>
function openCodeViewer(filePath) {
  document.getElementById('code-viewer-modal').style.display = 'block';
  document.getElementById('code-viewer-title').textContent = filePath;
  document.getElementById('code-viewer-content').innerHTML = '<code>Loading...</code>';

  // Try Tauri invoke, fallback to message
  if (window.__TAURI__) {
    window.__TAURI__.core.invoke('read_source_file', { path: filePath })
      .then(content => {
        const lines = content.split('\\n');
        const numbered = lines.map((line, i) => {
          const num = String(i + 1).padStart(4, ' ');
          const escaped = line.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
          return '<span style="color:#666;user-select:none;">' + num + '</span>  ' + escaped;
        }).join('\\n');
        document.getElementById('code-viewer-content').innerHTML = '<code>' + numbered + '</code>';
      })
      .catch(err => {
        document.getElementById('code-viewer-content').innerHTML = '<code style="color:#f44336;">Error: ' + err + '</code>';
      });
  } else {
    document.getElementById('code-viewer-content').innerHTML = '<code style="color:#ff9800;">Code viewing requires the Tauri desktop app.\\nRun: cargo tauri dev</code>';
  }
}
function closeCodeViewer() {
  document.getElementById('code-viewer-modal').style.display = 'none';
}
document.addEventListener('keydown', function(e) {
  if (e.key === 'Escape' && document.getElementById('code-viewer-modal').style.display !== 'none') {
    closeCodeViewer();
  }
});
</script>

`;
  html = html.replace('</body>', codeViewerModal + '</body>');
  console.log('Added code viewer modal');
}

// Step 4: Add "View Code" links to file cards in tracker (update tracker-tab.html inline)
// The tracker already has file names - we add click handlers via a small script
if (!html.includes('file-card-click-handler')) {
  const fileClickScript = `
<script id="file-card-click-handler">
// Add click-to-view-code on file cards
document.addEventListener('click', function(e) {
  const card = e.target.closest('.file-card');
  if (!card) return;
  const h3 = card.querySelector('h3');
  if (!h3) return;
  const fileName = h3.textContent.trim();
  if (fileName.endsWith('.fs') || fileName.endsWith('.c')) {
    openCodeViewer(fileName);
  }
});
</script>
`;
  html = html.replace('</body>', fileClickScript + '</body>');
  console.log('Added file card click handlers');
}

fs.writeFileSync(indexPath, html);
const finalLines = html.split('\n').length;
console.log(`Done! index.html is now ${finalLines} lines`);
