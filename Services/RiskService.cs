// RiskService.cs - rule-based, explainable clinical risk score with reasons and recommended action text.
using MaatriCare.Models;
namespace MaatriCare.Services;

public static class RiskService
{
    public static (string Level, List<string> Reasons, string RecommendedAction, int Score) Evaluate(Mother m)
    {
        int score = 0;
        var reasons = new List<string>();

        // Maternal baseline history rules
        if (m.Age < 18 || m.Age > 35)
        {
            score += 2;
            reasons.Add($"Age {m.Age} is outside the safe range (18–35)");
        }
        if (m.PreviousCSection)
        {
            score += 2;
            reasons.Add("Previous C-section delivery");
        }
        if (m.PreviousComplications >= 2)
        {
            score += 3;
            reasons.Add($"Multiple previous pregnancy complications ({m.PreviousComplications})");
        }
        else if (m.PreviousComplications == 1)
        {
            score += 1;
            reasons.Add("History of 1 previous pregnancy complication");
        }

        // Missed appointments check
        int missedAppts = m.Appointments?.Count(a => a.Status == "Missed") ?? 0;
        if (missedAppts > 0)
        {
            score += 2;
            reasons.Add($"Missed {missedAppts} scheduled ANC appointment(s)");
        }

        // Latest ANC visit vitals & labs
        var v = m.Visits?.OrderByDescending(x => x.VisitDate).FirstOrDefault();
        if (v == null)
        {
            score += 1;
            reasons.Add("No ANC visit recorded yet");
        }
        else
        {
            if (v.Systolic >= 140 || v.Diastolic >= 90)
            {
                score += 3;
                reasons.Add($"High blood pressure ({v.Systolic}/{v.Diastolic} mmHg)");
            }
            if (v.Hb < 7)
            {
                score += 3;
                reasons.Add($"Severe anaemia (Hb {v.Hb} g/dL)");
            }
            else if (v.Hb < 11)
            {
                score += 1;
                reasons.Add($"Mild anaemia (Hb {v.Hb} g/dL)");
            }
            if (v.BloodSugar >= 140)
            {
                score += 2;
                reasons.Add($"Elevated blood sugar ({v.BloodSugar} mg/dL)");
            }
            if (v.WeightKg.HasValue && v.WeightKg.Value < 42)
            {
                score += 2;
                reasons.Add($"Low maternal weight ({v.WeightKg:0.0} kg)");
            }
            else if (v.WeightKg.HasValue && v.WeightKg.Value > 95)
            {
                score += 1;
                reasons.Add($"High maternal weight ({v.WeightKg:0.0} kg)");
            }
            if (!string.IsNullOrWhiteSpace(v.UrineProtein) && v.UrineProtein != "Nil" && v.UrineProtein != "Trace")
            {
                score += (v.UrineProtein == "+2" || v.UrineProtein == "+3") ? 3 : 2;
                reasons.Add($"Positive urine protein ({v.UrineProtein}) — screen for pre-eclampsia");
            }
            if (v.FetalHeartbeat.HasValue && (v.FetalHeartbeat.Value < 110 || v.FetalHeartbeat.Value > 160))
            {
                score += 2;
                reasons.Add($"Abnormal fetal heartbeat ({v.FetalHeartbeat} bpm)");
            }
            if ((DateTime.Today - v.VisitDate).TotalDays > 45 && m.Week < 38 && m.Week > 0)
            {
                score += 1;
                reasons.Add("Last ANC visit was over 45 days ago");
            }
        }

        string level = score >= 5 ? "High" : score >= 2 ? "Medium" : "Low";
        string action = level switch
        {
            "High" => "Refer promptly to District Hospital / OB-GYN specialist, monitor BP & Hb weekly, and confirm emergency birth transport.",
            "Medium" => "Schedule closer clinic follow-up within 14 days, reinforce iron/calcium supplementation, and review warning signs with family.",
            _ => "Continue routine 4-visit antenatal schedule, balanced nutrition counselling, and institutional birth preparedness."
        };

        return (level, reasons, action, score);
    }
}
