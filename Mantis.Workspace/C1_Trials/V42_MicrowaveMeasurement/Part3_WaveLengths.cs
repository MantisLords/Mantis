using Mantis.Core.Calculator;
using Mantis.Core.FileImporting;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V42_Microwaves_Measurement;

public static class Part3_WaveLengths
{
    
    public static double _officialWaveLength = 0; // cm

    public static double OfficialWaveLength
    {
        get
        {
            if (_officialWaveLength == 0)
            {
                var reader = new SimpleTableProtocolReader("WaveLengths.csv");
                _officialWaveLength = reader.ExtractSingleValue<double>("officialWaveLength");
            }

            return _officialWaveLength;
        }
        set => _officialWaveLength = value;
    }

    public static void Process()
    {
        var reader = new SimpleTableProtocolReader("WaveLengths.csv");

        OfficialWaveLength = reader.ExtractSingleValue<double>("officialWaveLength");
        OfficialWaveLength.AddCommandAndLog("OfficialWaveLength","cm");
        
        // standingWaves
        var standingWavesRootCount = reader.ExtractSingleValue<int>("standingWavesRootCount");
        var standingWavesPos1 = reader.ExtractSingleValue<ErDouble>("standingWavesPos1");
        var standingWavesPos2 = reader.ExtractSingleValue<ErDouble>("standingWavesPos2");

        var standingWavesWaveLength = (standingWavesPos2 - standingWavesPos1) * 2 / standingWavesRootCount;
        standingWavesWaveLength.AddCommandAndLog("StandingWavesWaveLength","cm");
        
        // interferometer

        var interMaximaCount = reader.ExtractSingleValue<int>("interMaximaCount");
        var interPos1 = reader.ExtractSingleValue<ErDouble>("interPos1");
        var interPos2 = reader.ExtractSingleValue<ErDouble>("interPos2");

        var interWaveLength = (interPos2 - interPos1) * 2 / interMaximaCount;
        interWaveLength.AddCommandAndLog("InterferometerWaveLength","cm");
        

    }
}