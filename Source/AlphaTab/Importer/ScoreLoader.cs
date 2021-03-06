﻿using System;
using AlphaTab.IO;
using AlphaTab.Model;
using AlphaTab.Platform;

namespace AlphaTab.Importer
{
    /// <summary>
    /// The ScoreLoader enables you easy loading of Scores using all 
    /// available importers
    /// </summary>
    public class ScoreLoader
    {
        /// <summary>
        /// Loads a score asynchronously from the given datasource
        /// </summary>
        /// <param name="path">the source path to load the binary file from</param>
        /// <param name="success">this function is called if the Score was successfully loaded from the datasource</param>
        /// <param name="error">this function is called if any error during the loading occured.</param>
        public static void LoadScoreAsync(string path, Action<Score> success, Action<Exception> error)
        {
            IFileLoader loader = Environment.FileLoaders["default"]();
            loader.LoadBinaryAsync(path, data =>
            {
                Score score = null;
                try
                {
                    score = LoadScoreFromBytes(data);
                }
                catch (Exception e)
                {
                    error(e);
                }

                if (score != null)
                {
                    success(LoadScoreFromBytes(data));
                }

            }, error);
        }


        /// <summary>
        /// Loads a score synchronously from the given datasource
        /// </summary>
        /// <param name="path">the source path to load the binary file from</param>
        /// <returns></returns>
        public static Score LoadScore(string path)
        {
            IFileLoader loader = Environment.FileLoaders["default"]();
            var data = loader.LoadBinary(path);
            return LoadScoreFromBytes(data);
        }

        public static Score LoadScoreFromBytes(ByteArray data)
        {
            var importers = ScoreImporter.BuildImporters();

            Score score = null;
            using (MemoryStream ms = new MemoryStream(data))
            {
                foreach (var importer in importers)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        importer.Init(ms);
                        score = importer.ReadScore();
                        break;
                    }
                    catch (UnsupportedFormatException)
                    {
                        // ignore unsupported format
                    }
                }
            }

            if (score != null)
            {
                return score;
            }
            throw new NoCompatibleReaderFoundException();
        }
    }
}
