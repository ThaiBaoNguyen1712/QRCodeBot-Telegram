using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using QRCoder;

namespace QRCodeBot
{
    class Program
    {
        public static TelegramBotClient client;
        static async Task Main(string[] args)
        {
            client = new TelegramBotClient("7211327683:AAGWr0ZU7mg5TCHZLPHnMicUNvubRfTUa3w");
            using CancellationTokenSource cts = new();
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = await client.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }

        private static byte[] GenerateQRCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            if (messageText.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Welcome to Bao's bot Telegram! Please enter your content to generate the QR code. /Check" ,
                    cancellationToken: cancellationToken);
                return;
            }
            else
            {   
            // Generate QR code
            byte[] qrCodeImage = GenerateQRCode(messageText);

            // Send the image
            using (MemoryStream stream = new MemoryStream(qrCodeImage))
            {
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromStream(stream),
                    caption: "Here's your QR Code",
                    cancellationToken: cancellationToken);
            }
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception.ToString();
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}