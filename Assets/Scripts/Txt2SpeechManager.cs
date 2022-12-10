// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using UnityEngine;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine.Networking;
using System.Collections;


namespace LibAR
{

    /// <summary>
    /// Enables text to speech using the Windows 10 <see cref="SpeechSynthesizer"/> class.
    /// </summary>
    /// <remarks>
    /// <see cref="SpeechSynthesizer"/> generates speech as a <see cref="SpeechSynthesisStream"/>. 
    /// This class converts that stream into a Unity <see cref="AudioClip"/> and plays the clip using 
    /// the <see cref="AudioSource"/> you supply in the inspector. This allows you to position the voice 
    /// as desired in 3D space. One recommended approach is to place the AudioSource on an empty 
    /// GameObject that is a child of Main Camera and position it approximately 0.6 units above the 
    /// camera. This orientation will sound similar to Cortana's speech in the OS.
    /// </remarks>
    public class Txt2SpeechManager : MonoBehaviour
    {


        // Member Variables
        private SpeechSynthesizer synthesizer;
        private VoiceInfo voiceInfo;
        private string voiceName = "en-US-AriaNeural";

        private SpeechConfig speechConfig;
                
        // Static Helper Methods

        /// <summary>
        /// Converts two bytes to one float in the range -1 to 1 
        /// </summary>
        /// <param name="firstByte">
        /// The first byte.
        /// </param>
        /// <param name="secondByte">
        /// The second byte.
        /// </param>
        /// <returns>
        /// The converted float.
        /// </returns>
        static private float BytesToFloat(byte firstByte, byte secondByte)
        {
            // Convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);

            // Convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        /// <summary>
        /// Converts an array of bytes to an integer.
        /// </summary>
        /// <param name="bytes">
        /// The byte array.
        /// </param>
        /// <param name="offset">
        /// An offset to read from.
        /// </param>
        /// <returns>
        /// The converted int.
        /// </returns>
        static private int BytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }

        /// <summary>
        /// Dynamically creates an <see cref="AudioClip"/> that represents raw Unity audio data.
        /// </summary>
        /// <param name="name">
        /// The name of the dynamically generated clip.
        /// </param>
        /// <param name="audioData">
        /// Raw Unity audio data.
        /// </param>
        /// <param name="sampleCount">
        /// The number of samples in the audio data.
        /// </param>
        /// <param name="frequency">
        /// The frequency of the audio data.
        /// </param>
        /// <returns>
        /// The <see cref="AudioClip"/>.
        /// </returns>
        static private AudioClip ToClip(string name, float[] audioData, int sampleCount, int frequency)
        {
            // Create the audio clip
            var clip = AudioClip.Create(name, sampleCount, 1, frequency, false);

            // Set the data
            clip.SetData(audioData, 0);

            // Done
            return clip;
        }

        /// <summary>
        /// Converts raw WAV data into Unity formatted audio data.
        /// </summary>
        /// <param name="wavAudio">
        /// The raw WAV data.
        /// </param>
        /// <param name="sampleCount">
        /// The number of samples in the audio data.
        /// </param>
        /// <param name="frequency">
        /// The frequency of the audio data.
        /// </param>
        /// <returns>
        /// The Unity formatted audio data.
        /// </returns>
        static private float[] ToUnityAudio(byte[] wavAudio, out int sampleCount, out int frequency)
        {
            // Determine if mono or stereo
            int channelCount = wavAudio[22];     // Speech audio data is always mono but read actual header value for processing

            // Get the frequency
            frequency = BytesToInt(wavAudio, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wavAudio[pos] == 100 && wavAudio[pos + 1] == 97 && wavAudio[pos + 2] == 116 && wavAudio[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wavAudio[pos] + wavAudio[pos + 1] * 256 + wavAudio[pos + 2] * 65536 + wavAudio[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            sampleCount = (wavAudio.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (channelCount == 2) sampleCount /= 2;      // 4 bytes per sample (16 bit stereo)

            // Allocate memory (supporting left channel only)
            float[] unityData = new float[sampleCount];

            // Write to double array/s:
            int i = 0;
            while (pos < wavAudio.Length)
            {
                unityData[i] = BytesToFloat(wavAudio[pos], wavAudio[pos + 1]);
                pos += 2;
                if (channelCount == 2)
                {
                    pos += 2;
                }
                i++;
            }

            // Done
            return unityData;
        }


        // Internal Methods

        /// <summary>
        /// Logs speech text that normally would have been played.
        /// </summary>
        /// <param name="text">
        /// The speech text.
        /// </param>
        private void LogSpeech(string text)
        {
            Debug.LogFormat("Speech not supported in editor. \"{0}\"", text);
        }

        /// <summary>
        /// Executes a function that generates a speech stream and then converts and plays it in Unity.
        /// </summary>
        /// <param name="text">
        /// A raw text version of what's being spoken for use in debug messages when speech isn't supported.
        /// </param>
        private async Task PlaySpeech(string text, bool isSSML = false)
        {
            //Debug.Log("Playing Speech");

            if (String.IsNullOrEmpty(text)) return;
            AudioConfig audiocfg = AudioConfig.FromWavFileOutput("tmp-owlsey.mp3");
            synthesizer = new SpeechSynthesizer(speechConfig, audiocfg);

            try
            {
                // Need await, so most of this will be run as a new Task in its own thread.
                // This is good since it frees up Unity to keep running anyway.
                SpeechSynthesisResult speak = null;

                var responseText = text;
                if (!isSSML)
                {
                    // Synthesize spoken output
                    string responseSsml = $@"
                    <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                        <voice name='{voiceName}'>
                            {responseText}                                
                        </voice>
                    </speak>";


                    speak = await synthesizer.SpeakSsmlAsync(responseSsml);
                    Debug.Log(speak.Reason);

                    if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
                    {
                        Debug.Log(speak.Reason);

                        if (speak.Reason == ResultReason.Canceled)
                        {
                            var cancelDetails = SpeechSynthesisCancellationDetails.FromResult(speak);
                            Debug.Log(cancelDetails.ErrorDetails);
                        }
                        return;
                    }
                }
                else
                {

                    // Synthesize spoken output
                    speak = await synthesizer.SpeakTextAsync(responseText);
                    Debug.Log("Speak SSML: " + speak.Reason);

                    if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
                    {
                        Debug.Log(speak.Reason);
                        if (speak.Reason == ResultReason.Canceled)
                        {
                            var cancelDetails = SpeechSynthesisCancellationDetails.FromResult(speak);
                            Debug.Log(cancelDetails.ErrorDetails);
                        }
                        return;
                    }
                }
                //// Create buffer
                byte[] buffer = speak.AudioData;

                if (buffer.Length != 0)
                {
                    //    //var fbuffer = new float[buffer.Length];
                    //    //var count = 0;
                    //    //foreach (byte b in buffer)
                    //    //{
                    //    //    fbuffer[count] = (float)b;
                    //    //    count += 1;
                    //    //}

                    //    // Convert raw WAV data into Unity audio data
                    //    int sampleCount = 0;
                    //    int frequency = 0;
                    //    var unityData = ToUnityAudio(buffer, out sampleCount, out frequency);

                    //    // The remainder must be done back on Unity's main thread
                    //    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    //    {
                    //        // Convert to an audio clip
                    //    //    var clip = AudioClip.Create("Speech", sampleCount, 1, 1600, false);
                    //    //    clip.SetData(fbuffer, 0);

                    //        var clip = ToClip("Speech", unityData, sampleCount, frequency);

                    //            // Set the source on the audio clip
                    //        audioSource.clip = clip;

                    //            // Play audio
                    //        audioSource.Play();
                    //    }, false);
                    //}
                    if (System.IO.File.Exists("tmp-owlsey.mp3"))
                    {
                        string strFullpath = System.IO.Path.GetFullPath("tmp-owlsey.mp3");
                        string strHttp = string.Concat("file:///" + strFullpath);
                        StartCoroutine(GetAudioClip(audioSource, strHttp));
                        //Debug.Log("Playing mp3");
                    }

                    //Debug.Log("End Play Speech");

                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Speech generation problem: \"{0}\"", ex.Message);
            }
        }

        IEnumerator GetAudioClip(AudioSource audioSource, string audioUrl)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.Play();
                }
            }
        }

        // MonoBehaviour Methods
        void Start()
        {
            try
            {
                if (audioSource == null)
                {
                    Debug.LogError("An AudioSource is required and should be assigned to 'Audio Source' in the inspector.");
                }
                else
                {
                    // Configure speech service
                    speechConfig = SpeechConfig.FromSubscription(GlobalVars.cogServicesKey, GlobalVars.cogServiceRegion);
                    Debug.Log("Ready to use speech service in " + speechConfig.Region);

                    // Configure voice
                    speechConfig.SpeechSynthesisVoiceName = voiceName;
                    //speechConfig.OutputFormat = OutputFormat.Simple;
                    speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz192KBitRateMonoMp3);


                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not start Speech Synthesis");
                Debug.LogException(ex);
            }
        }

        // Public Methods

        /// <summary>
        /// Speaks the specified SSML markup using text-to-speech.
        /// </summary>
        /// <param name="ssml">
        /// The SSML markup to speak.
        /// </param>
        public async void SpeakSsml(string ssml)
        {
            // Make sure there's something to speak
            if (string.IsNullOrEmpty(ssml)) { return; }

            // Pass to helper method
            await PlaySpeech(ssml, true);
        }

        /// <summary>
        /// Speaks the specified text using text-to-speech.
        /// </summary>
        /// <param name="text">
        /// The text to speak.
        /// </param>
        public async void SpeakText(string text)
        {
            // Make sure there's something to speak
            if (string.IsNullOrEmpty(text)) { return; }

            // Pass to helper method
            await PlaySpeech(text, false);

        }

        [Tooltip("The audio source where speech will be played.")]
        [SerializeField]
        private AudioSource audioSource;

        /// <summary>
        /// Gets or sets the audio source where speech will be played.
        /// </summary>
        /// 
        // Inspector Variables    
        public AudioSource AudioSource { get { return audioSource; } set { audioSource = value; } }


    }

}