﻿using Mantis.Core.Calculator;
using Mantis.Core.TexIntegration;

namespace Mantis.Workspace.C1_Trials.V46_Radioactivity.Data_Smailagic_Karb;

public class V46_NullEffectAndActivity
{
    public static ErDouble _sampleActivity;
    public static ErDouble ActivityOfCsSample => _sampleActivity;
    public static void Process()
    {
        ErDouble timePassed = new ErDouble(46.6, 0.1);
        double CsHalfLife = 30.2;
        double csActivityPast = 3.7;
        double nullEffect = 198;
        double NullEffectInLead = 85;
        csActivityPast.AddCommandAndLog("ActivityPast","MBq");
        ErDouble activity = 3.7 * ErDouble.Exp(-Math.Log(2) * timePassed / CsHalfLife);
        activity.AddCommandAndLog("ActivityNow","MBq");
        _sampleActivity = activity;
        nullEffect.AddCommandAndLog("NullEffect");
        NullEffectInLead.AddCommandAndLog("NullEffectInLead");
        (100*NullEffectInLead/nullEffect).AddCommandAndLog("NullEffectPercentage","%");
        
    }
}