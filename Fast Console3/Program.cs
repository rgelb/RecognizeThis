using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting;
using SoundFingerprinting.DAO.Data;

string referenceFile = @"assets\ReferenceSound.wav";
string fileToCompare = @"assets\FileToCompare.wav";

var recognizer = new RecognizeFile();
await recognizer.StoreForLaterRetrieval(referenceFile);

TrackData match = await recognizer.GetBestMatchForSong(fileToCompare);

if (match == null) {
    Console.WriteLine("No Matches");
} else {
    Console.WriteLine(match.Id);
}



class RecognizeFile
{
    private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
    private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library

    public async Task StoreForLaterRetrieval(string file) {
        var track = new TrackInfo("GBBKS1200164", "Skyfall", "Adele");

        // create fingerprints
        var avHashes = await FingerprintCommandBuilder.Instance
                                    .BuildFingerprintCommand()
                                    .From(file)
                                    .UsingServices(audioService)
                                    .Hash();

        // store hashes in the database for later retrieval
        modelService.Insert(track, avHashes);
    }

    public async Task<TrackData> GetBestMatchForSong(string file) {
        int secondsToAnalyze = 10; // number of seconds to analyze from query file
        int startAtSecond = 0; // start at the begining

        // query the underlying database for similar audio sub-fingerprints
        var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                             .From(file, secondsToAnalyze, startAtSecond)
                                             .UsingServices(modelService, audioService)
                                             .Query();

        return queryResult.BestMatch?.Audio?.Track;
    }
}

