using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Graduation_Application.IServices
{
    /// <summary>
    /// Service for transcribing speech from an audio file into text.
    /// </summary>
    public interface ISpeechToTextService
    {
        /// <summary>
        /// Transcribes the provided audio file into text.
        /// </summary>
        /// <param name="audioFile">The audio file to transcribe.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transcribed text.</returns>
        Task<string> TranscribeAsync(IFormFile audioFile, CancellationToken cancellationToken = default);
    }
}
