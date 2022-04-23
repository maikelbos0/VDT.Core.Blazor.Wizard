﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VDT.Core.XmlConverter.Markdown {
    /// <summary>
    /// Tracks trailing line terminators when writing Markdown content to a <see cref="TextWriter"/>; used to determine if additional line terminators are needed in later content
    /// </summary>
    public class TrailingNewLineTracker {
        private readonly Dictionary<string, object?> additionalData;

        /// <summary>
        /// Amount of trailing line terminators
        /// </summary>
        public int NewLineCount {
            get {
                if (additionalData.TryGetValue(nameof(NewLineCount), out var countObj) && countObj is int count) {
                    return count;
                }

                return 0;
            }
            private set {
                additionalData[nameof(NewLineCount)] = value;
            }
        }

        /// <summary>
        /// Construct a trailing new line tracker
        /// </summary>
        /// <param name="additionalData">Additional data for the current conversion</param>
        public TrailingNewLineTracker(Dictionary<string, object?> additionalData) {
            this.additionalData = additionalData;
        }

        /// <summary>
        /// Writes a string to the text stream
        /// </summary>
        /// <param name="writer">Writer to write the string to</param>
        /// <param name="value">String to write</param>
        public void Write(TextWriter writer, string value) {
            writer.Write(value);
            UpdateNewLineCount(value);
        }

        /// <summary>
        /// Writes a string to the text stream, followed by a line terminator
        /// </summary>
        /// <param name="writer">Writer to write the string to</param>
        /// <param name="value">String to write</param>
        public void WriteLine(TextWriter writer, string value) {
            writer.WriteLine(value);
            UpdateNewLineCount(value, 1);
        }

        /// <summary>
        /// Writes a line terminator to the text stream
        /// </summary>
        /// <param name="writer">Writer to write the line terminator to</param>
        public void WriteLine(TextWriter writer) {
            writer.WriteLine();
            NewLineCount++;
        }

        private void UpdateNewLineCount(string value, int additionalNewLineCount = 0) {
            var values = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var newLineCount = values.Reverse().TakeWhile(s => string.IsNullOrEmpty(s)).Count();

            if (newLineCount == values.Length) {
                NewLineCount += newLineCount - 1 + additionalNewLineCount;
            }
            else { 
                NewLineCount = newLineCount + additionalNewLineCount;
            }
        }
    }
}
