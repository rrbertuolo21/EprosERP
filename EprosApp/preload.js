const { contextBridge, ipcRenderer } = require('electron');

// Exposição segura de APIs e canais de IPC para o frontend Nuxt 3 (prevenindo RCE)
contextBridge.exposeInMainWorld('electronAPI', {
  send: (channel, data) => {
    const validChannels = ['toMain'];
    if (validChannels.includes(channel)) {
      ipcRenderer.send(channel, data);
    }
  },
  receive: (channel, func) => {
    const validChannels = ['fromMain'];
    if (validChannels.includes(channel)) {
      // Executa de forma encapsulada removendo a exposição direta do event object
      ipcRenderer.on(channel, (event, ...args) => func(...args));
    }
  }
});
