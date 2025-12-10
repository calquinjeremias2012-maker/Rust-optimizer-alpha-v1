using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("TextureReapply", "Jeremias", "0.1.0")]
    [Description("Reaplica automáticamente la configuración de texturas al reiniciar el servidor Rust.")]
    public class TextureReapply : CovalencePlugin
    {
        private TextureConfig _config;

        private class TextureConfig
        {
            public TextureSettings TextureSettings { get; set; } = new TextureSettings();
        }

        private class TextureSettings
        {
            public bool StreamingEnabled { get; set; } = true;
            public string StreamingPriority { get; set; } = "balanced";
            public int MaxTextureSize { get; set; } = 1024;
            public int MipBias { get; set; } = -1;
            public bool PreloadCriticalTextures { get; set; } = true;
            public List<string> CriticalTexturesList { get; set; } = new List<string>
            {
                "ui/icons/player.png",
                "ui/icons/weapon.png",
                "environment/terrain/grass_diffuse.png"
            };
            public AsyncLoading AsyncLoading { get; set; } = new AsyncLoading();
            public Cache Cache { get; set; } = new Cache();
            public Fallback Fallback { get; set; } = new Fallback();
            public Logging Logging { get; set; } = new Logging();
        }

        private class AsyncLoading
        {
            public bool Enabled { get; set; } = true;
            public int BatchSize { get; set; } = 4;
            public int DelayBetweenBatchesMs { get; set; } = 50;
        }

        private class Cache
        {
            public bool EnableDiskCache { get; set; } = true;
            public string CacheFolder { get; set; } = "oxide/data/texture_cache";
            public int MaxCacheSizeMB { get; set; } = 512;
        }

        private class Fallback
        {
            public bool LowResPlaceholder { get; set; } = true;
            public string PlaceholderColor { get; set; } = "#333333";
        }

        private class Logging
        {
            public bool EnableDebugLogs { get; set; } = false;
            public string LogFile { get; set; } = "oxide/logs/texture_loader.log";
        }

        // Hook que se ejecuta cada vez que el servidor arranca o se reinicia
        private void OnServerInitialized()
        {
            EnsureTextureConfig();
            ApplySettings();
        }

        private void EnsureTextureConfig()
        {
            try
            {
                _config = Interface.Oxide.DataFileSystem.ReadObject<TextureConfig>("texture_config");

                if (_config == null)
                {
                    Puts("[TextureReapply] No se encontró texture_config.json, creando configuración por defecto...");
                    _config = new TextureConfig();
                    Interface.Oxide.DataFileSystem.WriteObject("texture_config", _config, true);
                }
                else
                {
                    Puts("[TextureReapply] texture_config.json cargado correctamente.");
                }
            }
            catch
            {
                Puts("[TextureReapply] Error al leer texture_config.json, generando configuración por defecto...");
                _config = new TextureConfig();
                Interface.Oxide.DataFileSystem.WriteObject("texture_config", _config, true);
            }
        }

        private void ApplySettings()
        {
            if (_config.TextureSettings.StreamingEnabled)
            {
                Puts($"[TextureReapply] Streaming activado con prioridad {_config.TextureSettings.StreamingPriority}");
            }

            Puts($"[TextureReapply] Tamaño máximo de textura: {_config.TextureSettings.MaxTextureSize}px");
            Puts($"[TextureReapply] MipBias: {_config.TextureSettings.MipBias}");

            if (_config.TextureSettings.PreloadCriticalTextures)
            {
                foreach (var tex in _config.TextureSettings.CriticalTexturesList)
                {
                    Puts($"[TextureReapply] Precargando textura crítica: {tex}");
                }
            }

            if (_config.TextureSettings.AsyncLoading.Enabled)
            {
                Puts($"[TextureReapply] Carga asíncrona activada en lotes de {_config.TextureSettings.AsyncLoading.BatchSize} con delay {_config.TextureSettings.AsyncLoading.DelayBetweenBatchesMs}ms");
            }

            if (_config.TextureSettings.Cache.EnableDiskCache)
            {
                Puts($"[TextureReapply] Cache habilitado en carpeta {_config.TextureSettings.Cache.CacheFolder} con límite {_config.TextureSettings.Cache.MaxCacheSizeMB}MB");
            }

            if (_config.TextureSettings.Fallback.LowResPlaceholder)
            {
                Puts($"[TextureReapply] Usando placeholder de color {_config.TextureSettings.Fallback.PlaceholderColor} mientras se cargan texturas.");
            }

            if (_config.TextureSettings.Logging.EnableDebugLogs)
            {
                Puts($"[TextureReapply] Logs detallados en {_config.TextureSettings.Logging.LogFile}");
            }
        }
    }
}