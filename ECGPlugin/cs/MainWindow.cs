using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SamplePlugin.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private string[] imagePaths; // Пути к изображениям для анимации сердечного ритма
        private int heartbeatImageIndex; // Индекс текущего изображения в массиве imagePaths
        private Plugin plugin; // Ссылка на основной плагин
        private bool isAnimating; // Флаг, указывающий, идет ли анимация
        private bool isLooping; // Флаг, указывающий, идет ли бесконечная анимация
        private Thread? animationThread; // Поток для основной анимации пульса
        private Thread? heartbeatAnimationThread; // Поток для анимации изображения сердечного ритма
        private float[] heartRateData; // Массив данных пульса для отображения на графике
        private int heartRateIndex; // Индекс текущего значения в массиве heartRateData
        private float timeSinceLastUpdate; // Время, прошедшее с последнего обновления данных пульса
        private int spikeCount; // Счетчик пиков пульса
        private float timeSinceLastSpike; // Время, прошедшее с последнего пика
        private Configuration configuration; // Конфигурация плагина
        private int animationCycleCount; // Счетчик циклов анимации
        private int cyclesPerMinute; // Пульс в ударах в минуту (BPM)
        private Stopwatch stopwatch; // Таймер для вычисления времени, прошедшего с начала анимации
        private List<long> cycleTimes; // Список времени для вычислений BPM

        private float lastSpikeHeight; // Сохранение предыдущего значения высоты пика
        private float lastSmallSpikeHeight; // Сохранение предыдущего значения высоты малого пика

        public MainWindow(Plugin plugin, string[] imagePaths)
            : base("ECG Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.imagePaths = imagePaths; // Инициализация путей к изображениям
            heartbeatImageIndex = 0; // Инициализация индекса изображения
            this.plugin = plugin; // Инициализация ссылки на плагин
            this.configuration = plugin.Configuration; // Инициализация конфигурации плагина
            animationCycleCount = 0; // Инициализация счетчика циклов анимации
            cyclesPerMinute = 0; // Инициализация BPM
            cycleTimes = new List<long>(); // Инициализация списка времени
            stopwatch = new Stopwatch(); // Инициализация таймера
            StartAnimations(); // Запуск анимаций

            heartRateData = new float[135]; // Инициализация массива данных пульса
            heartRateIndex = 0; // Инициализация индекса данных пульса
            timeSinceLastUpdate = 0f; // Инициализация времени с последнего обновления
            spikeCount = 0; // Инициализация счетчика пиков
            timeSinceLastSpike = 0f; // Инициализация времени с последнего пика
            lastSpikeHeight = plugin.Configuration.SpikeHeight; // Сохранение текущего SpikeHeight
            lastSmallSpikeHeight = plugin.Configuration.SmallSpikeHeight; // Сохранение текущего SmallSpikeHeight
        }

        public void Dispose()
        {
            StopAnimations(); // Остановка всех анимаций
        }

        public void SetDefaultSize()
        {
            Size = new Vector2(450, 600); // Установка размера окна по умолчанию
        }

        public override void Draw()
        {
            var player = Plugin.ClientState.LocalPlayer as IPlayerCharacter; // Получение игрока
            if (player != null)
            {
                float healthPercent = (float)player.CurrentHp / player.MaxHp * 100; // Расчет процента здоровья игрока
                if (plugin.Configuration.SimulationMode == SimulationMode.SimulateCustomHP)
                {
                    healthPercent = plugin.Configuration.SimulatedHealthPercentage; // Использование симулированного здоровья
                }

                plugin.HeartRateMonitor.UpdateHeartRate(healthPercent); // Обновление данных пульса
                UpdateHealth(healthPercent); // Обновление состояния анимаций

                ImGui.Indent(27.5f);
                ImGui.Text($"Heart rate of {player.Name} is {(healthPercent == 0 ? 0 : cyclesPerMinute)} BPM"); // Отображение процента здоровья и пульса
                ImGui.Unindent(27.5f);

                ImGui.PushStyleColor(ImGuiCol.PlotLines, new Vector4(0f / 255f, 128f / 255f, 0f / 255f, 1f)); // Настройка стилей графика пульса
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(36f / 255f, 36f / 255f, 36f / 255f, 1f));
                ImGui.Indent(27.5f);
                ImGui.PlotLines("", ref heartRateData[0], 135, 0, null, -40, 40, new Vector2(plugin.Configuration.ECGWidth, 80)); // Отображение графика пульса
                ImGui.Unindent(27.5f);
                ImGui.PopStyleColor(2);

                ImGui.Spacing();

                var heartbeatImage = Plugin.TextureProvider.GetFromFile(imagePaths[heartbeatImageIndex]).GetWrapOrDefault(); // Получение изображения пульса
                if (heartbeatImage != null)
                {
                    float aspectRatio = (float)heartbeatImage.Width / heartbeatImage.Height; // Вычисление соотношения сторон изображения
                    float windowWidth = ImGui.GetWindowWidth(); // Получение ширины окна
                    float availableWidth = windowWidth - 55f; // Доступная ширина для изображения
                    float imageHeight = availableWidth / aspectRatio; // Вычисление высоты изображения

                    if (imageHeight > 80)
                    {
                        imageHeight = 80; // Ограничение высоты изображения
                        availableWidth = imageHeight * aspectRatio; // Пересчет доступной ширины
                    }

                    availableWidth *= plugin.Configuration.ImageSize; // Применение масштаба изображения
                    imageHeight *= plugin.Configuration.ImageSize; // Применение масштаба изображения

                    float indent = 27.5f;
                    ImGuiHelpers.ScaledIndent(indent);
                    ImGui.Image(heartbeatImage.ImGuiHandle, new Vector2(availableWidth, imageHeight),
                                new Vector2(0, 0), new Vector2(1, 1),
                                new Vector4(1.0f, 1.0f, 1.0f, plugin.Configuration.ImageTransparency)); // Отображение изображения с учетом прозрачности
                }
            }
            else
            {
                ImGui.Text("Player not found."); // Сообщение, если игрок не найден
            }
        }

        private void StartAnimations()
        {
            if (isAnimating) return; // Если анимации уже запущены, ничего не делать

            isAnimating = true;
            isLooping = true;
            animationCycleCount = 0; // Сброс счетчика циклов анимации
            cyclesPerMinute = 0; // Сброс BPM
            stopwatch.Start(); // Запуск таймера

            animationThread = new Thread(() =>
            {
                while (isAnimating)
                {
                    var player = Plugin.ClientState.LocalPlayer as IPlayerCharacter; // Получение игрока
                    float healthPercent = 100f; // По умолчанию процент здоровья 100

                    if (player != null)
                    {
                        healthPercent = (float)player.CurrentHp / player.MaxHp * 100; // Расчет процента здоровья

                        if (plugin.Configuration.SimulationMode == SimulationMode.SimulateCustomHP)
                        {
                            healthPercent = plugin.Configuration.SimulatedHealthPercentage; // Использование симулированного значения
                        }
                    }

                    if (healthPercent == 0)
                    {
                        plugin.Configuration.SpikeHeight = 0; // Установка высоты пиков в 0
                        plugin.Configuration.SmallSpikeHeight = 0; // Установка высоты маленького пика в 0
                    }
                    else
                    {
                        if (plugin.Configuration.SpikeHeight == 0 && plugin.Configuration.SmallSpikeHeight == 0)
                        {
                            plugin.Configuration.SpikeHeight = lastSpikeHeight; // Восстановление высоты пиков
                            plugin.Configuration.SmallSpikeHeight = lastSmallSpikeHeight; // Восстановление высоты маленького пика
                        }
                    }

                    int spikeInterval = (int)Lerp(plugin.Configuration.SpikeIntervalAt1Percent, plugin.Configuration.SpikeIntervalAt100Percent, 1 - healthPercent / 100f); // Вычисление интервала пиков
                    configuration.SpikeInterval = spikeInterval;

                    configuration.AnimationSpeedMs = Lerp(configuration.MinBeatingSpeed, configuration.MaxBeatingSpeed, healthPercent / 100f); // Вычисление скорости анимации
                    configuration.PauseLengthMs = Lerp(configuration.MinPauseBetweenBeats, configuration.MaxPauseBetweenBeats, healthPercent / 100f); // Вычисление длины паузы между ударами
                    configuration.Save(); // Сохранение конфигурации

                    int beatingSpeed = (int)configuration.AnimationSpeedMs; // Задержка между кадрами анимации
                    int pauseLength = (int)configuration.PauseLengthMs; // Длина паузы между анимациями
                    Thread.Sleep(beatingSpeed); // Задержка между кадрами анимации

                    plugin.HeartRateMonitor.UpdateHeartRate(healthPercent); // Обновление данных пульса

                    float updateInterval = Math.Max(plugin.Configuration.MinUpdateInterval, plugin.Configuration.MaxUpdateInterval); // Вычисление интервала обновления данных
                    timeSinceLastUpdate += beatingSpeed / 1000f;
                    timeSinceLastSpike += beatingSpeed / 1000f;

                    if (timeSinceLastUpdate >= updateInterval)
                    {
                        timeSinceLastUpdate = 0f;

                        for (int i = 0; i < plugin.Configuration.HeartRateDataSize - 1; i++)
                        {
                            heartRateData[i] = heartRateData[i + 1]; // Сдвиг данных пульса
                        }

                        float y = healthPercent == 0 ? 0 : GenerateHeartRateSpike(heartRateIndex); // Генерация значения пика пульса
                        heartRateData[plugin.Configuration.HeartRateDataSize - 1] = y * 20;
                        heartRateIndex++;

                        if (y == plugin.Configuration.SpikeHeight * 2)
                        {
                            spikeCount++; // Увеличение счетчика пиков
                        }
                    }
                }
            });

            heartbeatAnimationThread = new Thread(() =>
            {
                while (isLooping)
                {
                    int beatingSpeed = (int)configuration.AnimationSpeedMs; // Скорость анимации
                    int pauseLength = (int)configuration.PauseLengthMs; // Длина паузы между анимациями
                    if (plugin.Configuration.SimulatedHealthPercentage > 0)
                    {
                        for (int i = 0; i < imagePaths.Length; i++)
                        {
                            heartbeatImageIndex = i; // Переключение изображения
                            Thread.Sleep(beatingSpeed); // Задержка между кадрами
                        }
                        Thread.Sleep(pauseLength); // Пауза после завершения цикла
                    }

                    animationCycleCount++;
                    cycleTimes.Add(stopwatch.ElapsedMilliseconds); // Добавление времени текущего цикла
                    if (cycleTimes.Count > 1)
                    {
                        long elapsed = cycleTimes.Last() - cycleTimes.First(); // Вычисление времени между циклами
                        cyclesPerMinute = (int)(60000.0 / (elapsed / (cycleTimes.Count - 1))); // Вычисление BPM

                        cyclesPerMinute = Math.Clamp(cyclesPerMinute, (int)plugin.Configuration.MinHeartRate, (int)plugin.Configuration.MaxHeartRate); // Ограничение BPM

                        cycleTimes.Clear(); // Очистка списка времени
                        cycleTimes.Add(stopwatch.ElapsedMilliseconds); // Добавление текущего времени
                    }
                    else
                    {
                        cyclesPerMinute = 0; // Установка BPM в 0, если недостаточно данных
                    }
                }
            });

            animationThread.Start(); // Запуск потока анимации
            heartbeatAnimationThread.Start(); // Запуск потока анимации изображения
        }

        private void StopAnimations()
        {
            isAnimating = false; // Остановка основной анимации
            isLooping = false; // Остановка бесконечной анимации
            animationThread?.Join(); // Ожидание завершения потока анимации
            heartbeatAnimationThread?.Join(); // Ожидание завершения потока анимации изображения
            stopwatch.Stop(); // Остановка таймера
        }

        private void UpdateHealth(float healthPercent)
        {
            if (healthPercent > 0 && !isAnimating)
            {
                StartAnimations(); // Запуск анимаций, если здоровье больше 0
            }
        }

        private float GenerateHeartRateSpike(int index)
        {
            int spikePhase = index % plugin.Configuration.SpikeInterval; // Определение фазы пика
            return spikePhase switch
            {
                0 => plugin.Configuration.SmallSpikeHeight, // Малый пик
                2 => plugin.Configuration.SpikeHeight, // Большой пик
                4 => plugin.Configuration.SpikeHeight * 2, // Двойной пик
                6 => -plugin.Configuration.SpikeHeight * 1.4f, // Негативный пик
                8 => plugin.Configuration.SmallSpikeHeight * 0.3f, // Малый отрицательный пик
                10 => -plugin.Configuration.SmallSpikeHeight * 0.3f, // Малый отрицательный пик
                12 => 0, // Нулевой пик
                _ => 0 // По умолчанию нулевой пик
            };
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t; // Линейная интерполяция для вычисления промежуточных значений
    }
}
