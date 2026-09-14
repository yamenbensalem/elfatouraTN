// Triggers a browser download of an in-memory file (used by the "Export Excel" buttons).
// Must live in a real static file — a <script> written inside a .razor component's markup
// is never executed by the browser (Blazor Server inserts DOM via its own diffing, not
// innerHTML/appendChild). See Web_GestCom/CLAUDE.md "Known Pitfalls".
window.downloadFileFromBytes = (fileName, base64Content, contentType) => {
    const byteChars = atob(base64Content);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const blob = new Blob([new Uint8Array(byteNumbers)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};
