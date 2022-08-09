using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBOT
{
    class Program
    {
        public static string token = System.IO.File.ReadAllText(@"C:\Users\Админ\source\repos\TelegaBot\TelegaBot\bin\Debug\net6.0\token.txt");
        static ITelegramBotClient bot = new TelegramBotClient(token);
        public static string PathFile = @"C:\Users\Админ\source\repos\TelegaBot\TelegaBot\bin\Debug\net6.0\documents\";
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Random r = new Random();
            if (update.Type == UpdateType.Message && update.Message!.Text != null)
            {
                await HandleMessage(botClient, update.Message);
                return;
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery!);
                return;
            }
            if (update.Message!.Document != null)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {update.Message.Chat.FirstName} {update.Message.Caption} {update.Message.Type}");
                DownLoadDocument(update.Message.Document.FileId, update.Message.Document.FileName!);
            }
            if (update.Message.Voice != null)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {update.Message.Chat.FirstName} {update.Message.Caption} {update.Message.Type}");
                DownLoadDocument(update.Message.Voice.FileId, $"voice{r.Next(1, 1000000)}.mp3");
            }
            if (update.Message.Photo != null)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {update.Message.Chat.FirstName} {update.Message.Caption}{update.Message.Type}");
                DownLoadDocument(update.Message.Photo.Last().FileId, $"photo{r.Next(1, 1000000)}.jpeg");
            }
        }
        static async void DownLoadDocument(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            using (var fileStream = new FileStream(PathFile + path, FileMode.Create))
                await bot.DownloadFileAsync(file.FilePath!, fileStream);
        }
        static async Task HandleMessage(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                    new KeyboardButton[]{"Купить тариф"},
                    new KeyboardButton[]{"Узнать о тарифах"}
                })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать! Здесь Вы можете очень быстро и выгодно купить секретные тарифы мобильного оператора МТС.", replyMarkup: keyboard);
                return;
            }
            if (message.Text == "Купить тариф")
            {
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                    new KeyboardButton[]{"Купить ТП Smart для своих"},
                    new KeyboardButton[]{"Купить ТП Диллер"}
                })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите тарифный план", replyMarkup: keyboard);
                return;
            }
            if (message.Text == "Узнать о тарифах")
            {
                InlineKeyboardMarkup keyboard = new(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Диллер","Диллер"),
                    InlineKeyboardButton.WithCallbackData("Smart для своих","Smart для своих")
                });
                await botClient.SendTextMessageAsync(message.Chat.Id, "Нажмите на нужный тарифный план", replyMarkup: keyboard);
                return;
            }
            if (message.Text == "Купить ТП Smart для своих")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для получения секретного кода необходимо оплатить 2990 рублей." +
                "\nСпособы оплаты:\n-по номеру телефона 8(977)-796-14-55\n-по номеру карты 2202 2015 9684 1193" +
                "\nОтправьте боту в чат скрин или чек оплаты" +
                "\nПосле подтверждения оплаты Вам будет предоставлен код." +
                "\nДалее действуйте по инструкции.");
                using (var fileStream = new FileStream("OpisanieSmart.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await botClient.SendPhotoAsync(
                        message.Chat.Id,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream));
                }
            }
            if (message.Text == "Купить ТП Диллер")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Для получения секретного кода необходимо оплатить 1990 рублей." +
                                    "\nСпособы оплаты:\n-по номеру телефона 8(977)-796-14-55\n-по номеру карты 2202 2015 9684 1193" +
                                    "\nОтправьте боту в чат скрин или чек оплаты" +
                                    "\nПосле подтверждения оплаты Вам будет предоставлен код." +
                                    "\nДалее действуйте по инструкции.");
                using (var fileStream = new FileStream("OpisanieDiller.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await botClient.SendPhotoAsync(
                        message.Chat.Id,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream));
                }
            }
            if (message != null)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {message.Chat.FirstName} {message.Chat.Id} {message.Text} {message.Type}");
                string fileName = message.Text!;
                var filePath = Path.Combine(PathFile, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    await using var stream = System.IO.File.Open(filePath, FileMode.Open);
                    message = await botClient.SendDocumentAsync(
                        chatId: message.Chat.Id,
                        document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(content: stream, fileName: fileName));
                }
            }
            if (message!.Text == "/file")
            {
                string[] allFiles = Directory.GetFiles(PathFile);
                foreach (string str in allFiles)
                {
                    FileInfo fI = new FileInfo(str);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"{fI.Name}");
                }
            }
        }
        static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "Диллер")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, $"Тарифный план {callbackQuery.Data}.");
                using (var fileStream = new FileStream("Diller1990.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await botClient.SendPhotoAsync(
                        callbackQuery.Message.Chat.Id,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream),
                        caption: "-500 минут по РФ\n-100 смс по РФ\n-Безлимитные звонки на МТС по РФ (На МТС минуты не расходуются)" +
                        $"\n-30ГБ интернета по РФ\n315 рублей/мес");
                }
            }
            if (callbackQuery.Data == "Smart для своих")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, $"Тарифный план {callbackQuery.Data}.");
                using (var fileStream = new FileStream("Smart2990.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await botClient.SendPhotoAsync(
                        callbackQuery.Message.Chat.Id,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(fileStream),
                        caption: "-500 минут\n-200 смс\n-Безлимитные звонки на МТС по РФ" +
                        $"\n-Безлимитный интернет по РФ\n400 рублей/мес");
                }
            }
            return;
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                => $"Ошибка телеграм АПИ: \n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken);
            Console.ReadLine();
        }
    }
}