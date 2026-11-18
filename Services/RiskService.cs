// RiskService.cs - rule-based, explainable risk score. Each rule adds points and a reason.
using MaatriCare.Models;
namespace MaatriCare.Services;

public static class RiskService
{
    public static (string Level, List<string> Reasons) Evaluate(Mother m)
    {
        int score = 0;
        var reasons = new List<string>();

        if (m.Age < 18 || m.Age > 35) { score += 2; reasons.Add($"Age {m.Age} is outside the safe range (18-35)"); }
        if (m.PreviousCSection) { score += 2; reasons.Add("Previous C-section"); }

        var v = m.Visits?.OrderByDescending(x => x.VisitDate).FirstOrDefault();
        if (v == null) { score += 1; reasons.Add("No ANC visit recorded yet"); }
        else
        {
            if (v.Systolic >= 140 || v.Diastolic >= 90) { score += 3; reasons.Add($"High blood pressure {v.Systolic}/{v.Diastolic}"); }
            if (v.Hb < 7) { score += 3; reasons.Add($"Severe anaemia (Hb {v.Hb})"); }
            else if (v.Hb < 11) { score += 1; reasons.Add($"Mild anaemia (Hb {v.Hb})"); }
            if (v.BloodSugar >= 140) { score += 2; reasons.Add($"High blood sugar ({v.BloodSugar})"); }
            if ((DateTime.Today - v.VisitDate).TotalDays > 45 && m.Week < 38 && m.Week > 0) { score += 1; reasons.Add("Last ANC visit was over 45 days ago"); }
        }
        string level = score >= 5 ? "High" : score >= 2 ? "Medium" : "Low";
        return (level, reasons);
    }
}
