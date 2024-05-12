using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgBot
{
    class Program
    {
        private static readonly Dictionary<long, string> _userFiles = new Dictionary<long, string>();
        private static readonly string _dropletPath1 = @"C:\Users\vados\OneDrive\Рабочий стол\Черно-белый.exe";
        private static readonly string _dropletPath2 = @"C:\Users\vados\OneDrive\Рабочий стол\180.exe";
        private static readonly string _dropletPath3 = @"C:\Users\vados\OneDrive\Рабочий стол\Contrast.exe";
        private static readonly string _dropletPath4 = @"C:\Users\vados\OneDrive\Рабочий стол\За стеклом.exe";
        private static readonly string _dropletPath5 = @"C:\Users\vados\OneDrive\Рабочий стол\Красочность.exe";
        private static readonly string _dropletPath6 = @"C:\Users\vados\OneDrive\Рабочий стол\Помехи.exe";
        private static readonly string _dropletPath7 = @"C:\Users\vados\OneDrive\Рабочий стол\Кристаллизация.exe";
        private static readonly string _dropletPath8 = @"C:\Users\vados\OneDrive\Рабочий стол\Воук.exe";
        private static readonly string _dropletPath9 = @"C:\Users\vados\OneDrive\Рабочий стол\Масляная краска.exe";

        static void Main(string[] args)
        {
            var client = new TelegramBotClient("7139490282:AAGIU93wUt7jcpims6xMCvTbgS2R7JsVsrU");
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessage(client, message),
                UpdateType.CallbackQuery => HandleCallbackQuery(client, update.CallbackQuery),
                _ => Task.CompletedTask
            };
            await handler;
        }

        private static async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message.Text != null)
            {
                switch (message.Text.Split(' ')[0])
                {
                    // Обработка команды /start
                    case "/start":
                        await client.SendTextMessageAsync(message.Chat.Id, "Привет! Я создан для того, чтобы быстро редактировать ваши фотографии с помощью различных эффектов! Предоставьте мне свое фото, чтобы я мог предложить варианты редактирования");
                        break;
                    default:
                        await client.SendTextMessageAsync(message.Chat.Id, "Извините, я не очень-то болтливый бот, мои возможности ограничены редактированием фотографий." +
                            "Если у вас есть какие-либо вопросы, напишите моему создателю @SonyaMarme1adova. Отправьте фотографию для обработки");
                        break;
                }
            }
            else if ((message.Document != null) || (message.Photo != null))
            {
                await HandleDocumentMessage(client, message);
            }
        }

        private static async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var userId = callbackQuery.From.Id;
            var data = callbackQuery.Data;

            await client.AnswerCallbackQueryAsync(callbackQuery.Id, $"Эффект {data} выбран!");
            if (!_userFiles.TryGetValue(userId, out var fileId))
            {
                await client.SendTextMessageAsync(chatId, "Ошибка: файл не был найден.");
                return;
            }

            string dropletPath = data switch
            {
                "1" => _dropletPath1,
                "2" => _dropletPath2,
                "3" => _dropletPath3,
                "4" => _dropletPath4,
                "5" => _dropletPath5,
                "6" => _dropletPath6,
                "7" => _dropletPath7,
                "8" => _dropletPath8,
                "9" => _dropletPath9,
                _ => null
            };

            var file = await client.GetFileAsync(fileId);
            var filePath = file.FilePath;

            // Сохраняем файл
            var sourceFilePath = Path.Combine(Path.GetTempPath(), $"{userId}_{fileId}.jpg");

            await using (var fileStream = new FileStream(sourceFilePath, FileMode.Create))
                await client.DownloadFileAsync(filePath, fileStream);

            string destinationFilePath = Path.Combine(Path.GetTempPath(), $"{userId}_{fileId}.jpg");

            // Запускаем дроплет с указанием файла
            var processStartInfo = new ProcessStartInfo
            {
                FileName = dropletPath,
                Arguments = $"{sourceFilePath}",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var process = Process.Start(processStartInfo))
                await process.WaitForExitAsync();


            if (System.IO.File.Exists(destinationFilePath))
            {
                using var editedFileStream = new FileStream(destinationFilePath, FileMode.Open);
                await client.SendPhotoAsync(chatId, new InputFileStream(editedFileStream, Path.GetFileName(destinationFilePath)));
            }
            else
                await client.SendTextMessageAsync(chatId, "Произошла ошибка во время редактирования фото.");
        }
        private static async Task HandleDocumentMessage(ITelegramBotClient client, Message message)
        {
            var chatId = message.Chat.Id;
            var userId = message.From.Id;

            if (message.Document != null)
            {
                _userFiles[userId] = message.Document.FileId;
                await client.SendTextMessageAsync(chatId, "Прекрасная фотография! Выберите один из следующих вариантов редактирования:",
                    replyMarkup: new InlineKeyboardMarkup(new[]{
            new[] {InlineKeyboardButton.WithCallbackData("Черно-белый", "1"),},
            new[]{InlineKeyboardButton.WithCallbackData("Переворот 180", "2"),},
            new[]{InlineKeyboardButton.WithCallbackData("Контраст", "3"),},
            new[]{InlineKeyboardButton.WithCallbackData("За стеклом", "4"),},
            new[]{InlineKeyboardButton.WithCallbackData("Красочность", "5")},
            new[]{InlineKeyboardButton.WithCallbackData("Помехи", "6")},
            new[]{InlineKeyboardButton.WithCallbackData("Кристаллизация", "7")},
            new[]{InlineKeyboardButton.WithCallbackData("Воук", "8")},
            new[]{InlineKeyboardButton.WithCallbackData("Масляная краска", "9")}}));
            }
            else
                await client.SendTextMessageAsync(chatId, "Пожалуйста, отправьте изображение в формате файла для применения эффектов.");
        }
        private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }

    }
}

