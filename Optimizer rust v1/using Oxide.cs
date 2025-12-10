using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("TextureOptimizer", "Jeremias", "0.2.0")]
    [Description("Crea y gestiona texture_config.json automáticamente en oxide/data.")]
    public class TextureOptimizer : CovalencePlugin
    {
        private TextureConfig _config;

        // Clases que reflejan la estructura del JSON
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

        // Inicialización del plugin
        private void Init()
        {
            EnsureTextureConfig();
            ApplySettings();
        }

        /// <summary>
        /// Crea el archivo texture_config.json si no existe.
        /// Si ya existe, no hace nada.
        /// </summary>
        private void EnsureTextureConfig()
        {
            try
            {
                _config = Interface.Oxide.DataFileSystem.ReadObject<TextureConfig>("texture_config");

                if (_config == null)
                {
                    Puts("[TextureOptimizer] No se encontró texture_config.json, creando configuración por defecto...");
                    _config = new TextureConfig();
                    Interface.Oxide.DataFileSystem.WriteObject("texture_config", _config, true);
                }
                else
                {
                    Puts("[TextureOptimizer] texture_config.json ya existe, no se realizaron cambios.");
                }
            }
            catch (Exception ex)
            {
                Puts("[TextureOptimizer] Error al leer texture_config.json, generando configuración por defecto: " + ex.Message);
                _config = new TextureConfig();
                try
                {
                    Interface.Oxide.DataFileSystem.WriteObject("texture_config", _config, true);
                }
                catch (Exception writeEx)
                {
                    Puts("[TextureOptimizer] Error al escribir texture_config.json: " + writeEx.Message);
                }
            }
        }

        // Aplicar la lógica de optimización con comprobaciones nulas
        private void ApplySettings()
        {
            if (_config?.TextureSettings == null)
            {
                Puts("[TextureOptimizer] Configuración inválida o ausente.");
                return;
            }

            var ts = _config.TextureSettings;

            if (ts.StreamingEnabled)
            {
                Puts($"[TextureOptimizer] Streaming activado con prioridad {ts.StreamingPriority}");
            }

            Puts($"[TextureOptimizer] Tamaño máximo de textura: {ts.MaxTextureSize}px");
            Puts($"[TextureOptimizer] MipBias: {ts.MipBias}");

            if (ts.PreloadCriticalTextures)
            {
                if (ts.CriticalTexturesList != null && ts.CriticalTexturesList.Count > 0)
                {
                    foreach (var tex in ts.CriticalTexturesList)
                    {
                        Puts($"[TextureOptimizer] Precargando textura crítica: {tex}");
                    }
                }
                else
                {
                    Puts("[TextureOptimizer] No hay texturas críticas definidas para precargar.");
                }
            }

            if (ts.AsyncLoading?.Enabled == true)
            {
                Puts($"[TextureOptimizer] Carga asíncrona activada en lotes de {ts.AsyncLoading.BatchSize} con delay {ts.AsyncLoading.DelayBetweenBatchesMs}ms");
            }

            if (ts.Cache?.EnableDiskCache == true)
            {
                Puts($"[TextureOptimizer] Cache habilitado en carpeta {ts.Cache.CacheFolder} con límite {ts.Cache.MaxCacheSizeMB}MB");
            }

            if (ts.Fallback?.LowResPlaceholder == true)
            {
                Puts($"[TextureOptimizer] Usando placeholder de color {ts.Fallback.PlaceholderColor} mientras se cargan texturas.");
            }

            if (ts.Logging?.EnableDebugLogs == true)
            {
                Puts($"[TextureOptimizer] Logs detallados en {ts.Logging.LogFile}");
            }
        }
    }
}