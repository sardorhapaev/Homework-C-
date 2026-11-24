using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OrderRefactoring;
using TextOptimization;

// === ЗАДАНИЕ 1: ПРОВЕРКА РЕФАКТОРИНГА ===
Console.WriteLine("=== ЗАДАНИЕ 1: РЕФАКТОРИНГ ===");

var testOrder = new Order
{
    Customer = new Customer { Email = "student@example.com" },
    Items = new List<OrderItem>
    {
        new() { Category = "electronics", Price = 600m, Quantity = 1 },
        new() { Category = "books", Price = 100m, Quantity = 2 }
    }
};

// Заглушки для БД и отправки писем, чтобы код запустился без реальной базы
var processor = new OrderProcessor(
    new OrderValidator(),
    new OrderCalculator(),
    new ConsoleOrderRepository(),
    new ConsoleNotificationService()
);

var result = processor.Process(testOrder);
Console.WriteLine(result.IsSuccess
    ? $"Успех! Итоговая сумма: {result.Total} (ожидалось 885.00)"
    : $"Ошибка: {result.ErrorMessage}");


// === ЗАДАНИЕ 2: ЗАМЕР СКОРОСТИ И ПАМЯТИ ===
Console.WriteLine("\n=== ЗАДАНИЕ 2: ОПТИМИЗАЦИЯ ===");

// Генерируем текст ~5.5 МБ
var sb = new StringBuilder();
string[] sampleWords = { "Lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit" };
var rand = new Random(42);
while (sb.Length < 5_500_000)
{
    sb.Append(sampleWords[rand.Next(sampleWords.Length)]).Append(' ');
}
string bigText = sb.ToString();

var analyzer = new TextAnalyzer();

// 1. Замер оптимизированной версии
GC.Collect();
long memBeforeFast = GC.GetTotalMemory(true);
var sw = Stopwatch.StartNew();
var fastRes = analyzer.AnalyzeFast(bigText);
sw.Stop();
long memAfterFast = GC.GetTotalMemory(false);
Console.WriteLine($"[Новая версия] Время: {sw.ElapsedMilliseconds} мс | Память: {Math.Max(0, (memAfterFast - memBeforeFast) / 1024)} КБ | Результат: {fastRes}");

// 2. Замер старой версии (подождите ~5-15 секунд)
Console.WriteLine("Запуск старой версии (ожидайте)...");
GC.Collect();
long memBeforeSlow = GC.GetTotalMemory(true);
sw.Restart();
var slowRes = analyzer.AnalyzeSlow(bigText);
sw.Stop();
long memAfterSlow = GC.GetTotalMemory(false);
Console.WriteLine($"[Старая версия] Время: {sw.ElapsedMilliseconds} мс | Память: {Math.Max(0, (memAfterSlow - memBeforeSlow) / 1024)} КБ | Результат: {slowRes}");


// --- Простые реализации интерфейсов для демонстрации работы ---
class ConsoleOrderRepository : IOrderRepository
{
    public void Save(Order order, decimal total) =>
        Console.WriteLine($"[БД] Заказ для {order.Customer.Email} сохранён.");
}

class ConsoleNotificationService : INotificationService
{
    public void SendOrderConfirmation(string email, decimal total) =>
        Console.WriteLine($"[Email] Письмо отправлено на {email}. Сумма: {total}");
}
