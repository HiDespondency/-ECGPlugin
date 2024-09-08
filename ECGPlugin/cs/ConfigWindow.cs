using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SamplePlugin.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        private Configuration configuration; // Ссылка на объект конфигурации плагина
        private Plugin plugin; // Ссылка на объект плагина

        public ConfigWindow(Plugin plugin) // Конструктор, инициализирующий окно конфигурации
            : base("ECG Configuration")
        {
            Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse; // Устанавливаем флаги окна
            Flags &= ~ImGuiWindowFlags.NoResize; // Позволяем изменять размер окна

            this.plugin = plugin; // Инициализируем плагин
            configuration = plugin.Configuration; // Инициализируем конфигурацию плагина
        }

        public void Dispose() { } // Метод для освобождения ресурсов (пока не используется)

        public override void PreDraw() // Метод вызывается перед отрисовкой окна
        {
            if (configuration.IsConfigWindowMovable) // Проверяем, можно ли двигать окно
            {
                Flags &= ~ImGuiWindowFlags.NoMove; // Разрешаем перемещение окна
            }
            else
            {
                Flags |= ImGuiWindowFlags.NoMove; // Запрещаем перемещение окна
            }
        }

        public void SetDefaultSize() // Метод для установки стандартного размера окна
        {
            Size = new Vector2(450, 800); // Устанавливаем размер окна
        }

        public override void Draw() // Метод отрисовки окна конфигурации
        {
            ImGui.Text("BPM Boundaries (BPM)"); // Раздел для настройки границ частоты сердечных сокращений (BPM)

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##MaxHeartRate"))  // Кнопка для сброса максимальной частоты сердечных сокращений на 180 BPM
            {
                configuration.MaxHeartRate = 180f; // Сброс до 180 BPM
                configuration.Save();
            }
            ImGui.SameLine();
            bool maxHeartRateLimitEnabled = configuration.MaxHeartRateLimitEnabled; // Чекбокс для включения/выключения ограничения максимальной частоты сердечных сокращений
            if (ImGui.Checkbox("##MaxHeartRateLimit", ref maxHeartRateLimitEnabled))  // Чекбокс для ограничения максимальной частоты
            {
                configuration.MaxHeartRateLimitEnabled = maxHeartRateLimitEnabled; // Включение/выключение ограничения
                configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("No Limit"); // Подсказка при наведении
            }
            ImGui.SameLine();
            var maxHeartRate = configuration.MaxHeartRate;
            if (configuration.MaxHeartRateLimitEnabled)
            {
                if (ImGui.InputFloat("Max Heart Rate", ref maxHeartRate, 1f, 10f, "%.1f"))  // Ползунок для ввода значения максимальной частоты
                {
                    configuration.MaxHeartRate = maxHeartRate; // Установка значения максимальной частоты
                    configuration.Save();
                }
            }
            else
            {
                if (ImGui.SliderFloat("Max Heart Rate", ref maxHeartRate, 60f, 220f, "%.1f"))  // Ползунок для изменения максимальной частоты
                {
                    configuration.MaxHeartRate = maxHeartRate; // Установка значения максимальной частоты
                    configuration.Save();
                }
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##MinHeartRate"))  // Кнопка для сброса минимальной частоты сердечных сокращений на 60 BPM
            {
                configuration.MinHeartRate = 60f; // Сброс до 60 BPM
                configuration.Save();
            }
            ImGui.SameLine();
            bool minHeartRateLimitEnabled = configuration.MinHeartRateLimitEnabled;
            if (ImGui.Checkbox("##MinHeartRateLimit", ref minHeartRateLimitEnabled))  // Чекбокс для ограничения минимальной частоты
            {
                configuration.MinHeartRateLimitEnabled = minHeartRateLimitEnabled; // Включение/выключение ограничения
                configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("No Limit"); // Подсказка при наведении
            }
            ImGui.SameLine();
            var minHeartRate = configuration.MinHeartRate;
            if (configuration.MinHeartRateLimitEnabled)
            {
                if (ImGui.InputFloat("Min Heart Rate", ref minHeartRate, 1f, 10f, "%.1f"))  // Ползунок для ввода значения минимальной частоты
                {
                    configuration.MinHeartRate = minHeartRate; // Установка значения минимальной частоты
                    configuration.Save();
                }
            }
            else
            {
                if (ImGui.SliderFloat("Min Heart Rate", ref minHeartRate, 40f, 100f, "%.1f"))  // Ползунок для изменения минимальной частоты
                {
                    configuration.MinHeartRate = minHeartRate; // Установка значения минимальной частоты
                    configuration.Save();
                }
            }
            ImGui.PopItemWidth();

            ImGui.Text(""); // Добавляем пустую строку для отступа

            ImGui.Text("ECG Settings"); // Раздел для настроек ECG

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##SpikeHeight"))  // Кнопка для сброса высоты спайка на 1.4
            {
                configuration.SpikeHeight = 1.4f; // Сброс до 1.4
                configuration.Save();
            }
            ImGui.SameLine();
            var spikeHeight = configuration.SpikeHeight;
            if (ImGui.InputFloat("Spike Height", ref spikeHeight, 0.1f, 1f, "%.1f"))  // Ползунок для ввода значения высоты спайка
            {
                configuration.SpikeHeight = spikeHeight; // Установка значения высоты спайка
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##SmallSpikeHeight"))  // Кнопка для сброса высоты маленького спайка на -0.3
            {
                configuration.SmallSpikeHeight = -0.3f; // Сброс до -0.3
                configuration.Save();
            }
            ImGui.SameLine();
            var smallSpikeHeight = configuration.SmallSpikeHeight;
            if (ImGui.InputFloat("Small Spike Height", ref smallSpikeHeight, 0.1f, 1f, "%.1f"))  // Ползунок для ввода значения высоты маленького спайка
            {
                configuration.SmallSpikeHeight = smallSpikeHeight; // Установка значения высоты маленького спайка
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##SpikeIntervalAt100Percent"))  // Кнопка для сброса интервала спайка при 100% HP на 19.3
            {
                configuration.SpikeIntervalAt100Percent = 19.3f; // Сброс до 19.3
                configuration.Save();
            }
            ImGui.SameLine();
            float spikeIntervalAt100Percent = configuration.SpikeIntervalAt100Percent;
            if (ImGui.InputFloat("Spike Interval at 100% HP", ref spikeIntervalAt100Percent, 0.1f, 1f, "%.1f"))  // Ползунок для ввода интервала спайка при 100% HP
            {
                if (spikeIntervalAt100Percent < 1) spikeIntervalAt100Percent = 1; // Минимальное значение 1
                configuration.SpikeIntervalAt100Percent = spikeIntervalAt100Percent; // Установка значения интервала спайка при 100% HP
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##SpikeIntervalAt1Percent"))  // Кнопка для сброса интервала спайка при 1% HP на 7
            {
                configuration.SpikeIntervalAt1Percent = 7f; // Сброс до 7
                configuration.Save();
            }
            ImGui.SameLine();
            float spikeIntervalAt1Percent = configuration.SpikeIntervalAt1Percent;
            if (ImGui.InputFloat("Spike Interval at 1% HP", ref spikeIntervalAt1Percent, 0.1f, 1f, "%.1f"))  // Ползунок для ввода интервала спайка при 1% HP
            {
                if (spikeIntervalAt1Percent < 1) spikeIntervalAt1Percent = 1; // Минимальное значение 1
                configuration.SpikeIntervalAt1Percent = spikeIntervalAt1Percent; // Установка значения интервала спайка при 1% HP
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.Text(""); // Добавляем пустую строку для отступа

            ImGui.Text("Animation Speed Settings"); // Раздел для настройки скорости анимации

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##Min Beating Speed"))  // Кнопка для сброса минимальной скорости биения на 6
            {
                configuration.MinBeatingSpeed = 6.0f; // Сброс до 6
                configuration.Save();
            }
            ImGui.SameLine();
            float minBeatingSpeed = configuration.MinBeatingSpeed;
            if (ImGui.InputFloat("Min Beating Speed", ref minBeatingSpeed, 1f, 10f, "%.1f"))  // Ползунок для ввода минимальной скорости биения
            {
                if (minBeatingSpeed < 0) minBeatingSpeed = 0; // Минимальное значение 0
                configuration.MinBeatingSpeed = minBeatingSpeed; // Установка значения минимальной скорости
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##Min Pause Between Beats"))  // Кнопка для сброса минимального интервала между ударами на 0
            {
                configuration.MinPauseBetweenBeats = 0.0f; // Сброс до 0
                configuration.Save();
            }
            ImGui.SameLine();
            float minPauseBetweenBeats = configuration.MinPauseBetweenBeats;
            if (ImGui.InputFloat("Min Pause Between Beats", ref minPauseBetweenBeats, 1f, 10f, "%.1f"))  // Ползунок для ввода минимального интервала между ударами
            {
                if (minPauseBetweenBeats < 0) minPauseBetweenBeats = 0; // Минимальное значение 0
                configuration.MinPauseBetweenBeats = minPauseBetweenBeats; // Установка значения минимального интервала
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##Max Beating Speed"))  // Кнопка для сброса максимальной скорости биения на 42
            {
                configuration.MaxBeatingSpeed = 42.0f; // Сброс до 42
                configuration.Save();
            }
            ImGui.SameLine();
            float maxBeatingSpeed = configuration.MaxBeatingSpeed;
            if (ImGui.InputFloat("Max Beating Speed", ref maxBeatingSpeed, 1f, 10f, "%.1f"))  // Ползунок для ввода максимальной скорости биения
            {
                if (maxBeatingSpeed < 0) maxBeatingSpeed = 0; // Минимальное значение 0
                configuration.MaxBeatingSpeed = maxBeatingSpeed; // Установка значения максимальной скорости
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##Max Pause Between Beats"))  // Кнопка для сброса максимального интервала между ударами на 140
            {
                configuration.MaxPauseBetweenBeats = 140.0f; // Сброс до 140
                configuration.Save();
            }
            ImGui.SameLine();
            float maxPauseBetweenBeats = configuration.MaxPauseBetweenBeats;
            if (ImGui.InputFloat("Max Pause Between Beats", ref maxPauseBetweenBeats, 1f, 10f, "%.1f"))  // Ползунок для ввода максимального интервала между ударами
            {
                if (maxPauseBetweenBeats < 0) maxPauseBetweenBeats = 0; // Минимальное значение 0
                configuration.MaxPauseBetweenBeats = maxPauseBetweenBeats; // Установка значения максимального интервала
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.Text(""); // Добавляем пустую строку для отступа

            ImGui.Text("Visual Settings"); // Раздел для визуальных настроек

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##ECGWidth"))  // Кнопка для сброса ширины ECG на 375
            {
                configuration.ECGWidth = 375f; // Сброс до 375
                configuration.Save();
            }
            ImGui.SameLine();
            float ecgWidth = configuration.ECGWidth;
            if (ImGui.SliderFloat("ECG Width", ref ecgWidth, 0.1f, 640.0f, "%.1f"))  // Ползунок для изменения ширины ECG
            {
                configuration.ECGWidth = ecgWidth; // Установка значения ширины ECG
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##ImageSize"))  // Кнопка для сброса размера изображения на 3.5
            {
                configuration.ImageSize = 3.5f; // Сброс до 3.5
                configuration.Save();
            }
            ImGui.SameLine();
            float imageSize = configuration.ImageSize;
            if (ImGui.SliderFloat("Image Size", ref imageSize, 0.1f, 6.0f, "%.1f"))  // Ползунок для изменения размера изображения
            {
                configuration.ImageSize = imageSize; // Установка значения размера изображения
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(100);
            if (ImGui.Button("D##ResetTransparency"))  // Кнопка для сброса прозрачности изображения на 100%
            {
                configuration.ImageTransparency = 1.0f; // Сброс до полной прозрачности
                configuration.Save();
            }
            ImGui.SameLine();
            float imageTransparency = configuration.ImageTransparency;
            if (ImGui.SliderFloat("Image Transparency", ref imageTransparency, 0.0f, 1.0f, "%.2f"))  // Ползунок для изменения прозрачности изображения
            {
                configuration.ImageTransparency = imageTransparency; // Установка значения прозрачности изображения
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.Text(""); // Добавляем пустую строку для отступа

            ImGui.PushItemWidth(100);
            bool simulateHealth = configuration.SimulationMode != SimulationMode.None; // Чекбокс для включения/выключения симуляции здоровья
            if (ImGui.Checkbox("##SimulateHealth", ref simulateHealth))  // Чекбокс для включения/выключения симуляции здоровья
            {
                configuration.SimulationMode = simulateHealth ? SimulationMode.SimulateCustomHP : SimulationMode.None; // Включение/выключение симуляции здоровья
                configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Toggle On/Off"); // Подсказка при наведении
            }
            ImGui.SameLine();
            int simulatedHealthPercentage = (int)configuration.SimulatedHealthPercentage;
            if (ImGui.SliderInt("Simulated HP%", ref simulatedHealthPercentage, 0, 100))  // Ползунок для изменения процентов симуляции здоровья
            {
                configuration.SimulatedHealthPercentage = simulatedHealthPercentage; // Установка процентов симуляции здоровья
                configuration.Save();
            }
            ImGui.PopItemWidth();

            ImGui.Text("");

            if (ImGui.Button("Main Window Default Size")) // Кнопки для установки размеров окон по умолчанию
            {
                ImGui.SetWindowSize("ECG Window", new Vector2(435, 445)); // Устанавливаем размер главного окна
            }
            if (ImGui.Button("Config Window Default Size"))
            {
                ImGui.SetWindowSize("ECG Configuration", new Vector2(305, 655)); // Устанавливаем размер окна конфигурации
            }

            if (ImGui.CollapsingHeader("DevData")) // Раздел для отладки данных
            {
                // Отображаем данные для отладки
                ImGui.Text($"Spike interval: {configuration.SpikeInterval}");
                ImGui.Text($"Current Beating Speed (ms): {configuration.AnimationSpeedMs}");
                ImGui.Text($"Current Pause Between Beats (ms): {configuration.PauseLengthMs}");
                ImGui.Text($"Heart Rate Data Size: {configuration.HeartRateDataSize}");
                ImGui.Text($"Min update interval: {configuration.MinUpdateInterval:F4}");
                ImGui.Text($"Max update interval: {configuration.MaxUpdateInterval:F4}");
            }
        }
    }
}
