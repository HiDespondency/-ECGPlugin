using System.Collections.Generic;

namespace SamplePlugin.Windows
{
    // Класс для мониторинга и обновления данных пульса
    public class HeartRateMonitor
    {
        // Конфигурация для мониторинга пульса
        private readonly Configuration config; // Конфигурация для управления параметрами пульса
        // Список данных пульса для хранения и обработки
        private readonly List<float> heartRateData; // Хранение данных пульса

        // Конструктор класса HeartRateMonitor
        public HeartRateMonitor(Configuration config)
        {
            this.config = config; // Инициализация конфигурации
            // Инициализация списка данных пульса с нулевыми значениями
            heartRateData = new List<float>(new float[this.config.HeartRateDataSize]); // Заполнение списка начальными нулями
        }

        // Метод для обновления данных пульса на основе процента здоровья
        public void UpdateHeartRate(float healthPercentage)
        {
            // Если процент здоровья равен 0, очищаем данные и добавляем 0
            if (healthPercentage == 0)
            {
                heartRateData.Clear(); // Очистка списка данных пульса
                heartRateData.Add(0); // Добавление значения 0
                return; // Завершение метода
            }

            // Вычисление интервала пиков на основе процента здоровья
            var spikeInterval = Lerp(config.SpikeIntervalAt100Percent, config.SpikeIntervalAt1Percent, 1 - healthPercentage / 100f); // Интервал между пиками
            config.SpikeInterval = (int)spikeInterval; // Обновление конфигурации интервала пиков

            // Вычисление максимального интервала обновления данных на основе процента здоровья
            var maxUpdateInterval = Lerp(0.010f, 0.0001f, 1 - healthPercentage / 100f); // Интервал обновления данных
            config.MaxUpdateInterval = (byte)maxUpdateInterval; // Обновление конфигурации максимального интервала обновления

            // Вычисление пульса на основе процента здоровья
            var heartRate = Lerp(config.MinHeartRate, config.MaxHeartRate, 1 - healthPercentage / 100f); // Вычисление текущего пульса

            // Если размер списка данных пульса достиг предела, удаляем старейший элемент
            if (heartRateData.Count >= config.HeartRateDataSize)
            {
                heartRateData.RemoveAt(0); // Удаление самого старого значения
            }

            // Добавляем новое значение пульса в список
            heartRateData.Add(heartRate); // Добавление нового значения
        }

        // Метод для получения текущих данных пульса
        public List<float> GetHeartRateData() => heartRateData; // Возвращение списка данных пульса

        // Линейная интерполяция для вычисления промежуточных значений
        private static float Lerp(float a, float b, float t) => a + (b - a) * t; // Интерполяция между значениями a и b на основе t
    }
}
