using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Concentus.Oggfile;
using Concentus.Structs;
using Microsoft.Bot.Builder;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using Newtonsoft.Json;
using WeatherBot.Models;

namespace WeatherBot.Services
{
    public class RecognitionServices
    {
        private const string subscriptionKey = "Your_Translator_Cognitive_Service_Key";

        private const string endpoint = "https://api.cognitive.microsofttranslator.com";

        public async Task<string> VoiceMessageRecognitionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string recognizedResult = "";
            string url = "";
            string key = "Your_Speech_Cognitive_Service_Key";
            string region = "westeurope";
            string filePath = "./Resources/VoiceMessage/";
            Guid fileOga = Guid.NewGuid();
            Guid fileWav = Guid.NewGuid();
            string fileExtensionOga = ".oga";
            string fileExtensionWav = ".wav";
            var config = SpeechConfig.FromSubscription(key, region);
            var attacmentsList = turnContext.Activity.Attachments;
            foreach (var item in attacmentsList)
            {
                url = item.ContentUrl;
            }

            using (var client = new WebClient())
            {
                var content = client.DownloadData(url);
                using (var fileStream = File.Create($"{filePath}{fileOga}{fileExtensionOga}"))
                {
                    await fileStream.WriteAsync(content);
                }
            }

            using (FileStream fileIn = new FileStream($"{filePath}{fileOga}{fileExtensionOga}", FileMode.Open))
            using (MemoryStream pcmStream = new MemoryStream())
            {
                OpusDecoder decoder = OpusDecoder.Create(48000, 1);
                OpusOggReadStream oggIn = new OpusOggReadStream(decoder, fileIn);
                while (oggIn.HasNextPacket)
                {
                    short[] packet = oggIn.DecodeNextPacket();
                    if (packet != null)
                    {
                        for (int i = 0; i < packet.Length; i++)
                        {
                            var bytes = BitConverter.GetBytes(packet[i]);
                            pcmStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                pcmStream.Position = 0;
                var wavStream = new RawSourceWaveStream(pcmStream, new WaveFormat(48000, 1));
                var sampleProvider = wavStream.ToSampleProvider();

                WaveFileWriter.CreateWaveFile16($"{filePath}{fileWav}{fileExtensionWav}", sampleProvider);

                using var audioConfig = AudioConfig.FromWavFileInput($"{filePath}{fileWav}{fileExtensionWav}");
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                var result = await recognizer.RecognizeOnceAsync();
                switch (result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        recognizedResult = result.Text;
                        break;
                    case ResultReason.NoMatch:
                        throw new Exception("Speech could not be recognized");
                }
            }
            DirectoryInfo di = new DirectoryInfo(filePath);
            var files = di.GetFiles();
            files.Where(p => p.Name.Contains(fileWav.ToString()) || p.Name.Contains(fileOga.ToString()))
                .ToList()
                .ForEach(p => p.Delete());
            return recognizedResult;
        }


        public async Task<string> Translator(ITurnContext turnContext, CancellationToken cancellation)
        {
            // Input and output languages are defined as parameters.
            string route = "/translate?api-version=3.0&from=uk&to=en";
            //string textToTranslate = "Погода Львів";
            string region = "westeurope";
            object[] body = new object[] { new { Text = turnContext.Activity.Text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                var rootList = new List<Translation>();
                string translationResult = "";

                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                var result = await response.Content.ReadAsStringAsync();

                var root = JsonConvert.DeserializeObject<List<TranslationResult>>(result);

                foreach (var item in root)
                {
                    rootList = item.translations;
                }
                foreach (var item in rootList)
                {
                    translationResult = item.text;
                }

                return translationResult;
            }

        }
    }
}
