// VaccineService.cs - builds the India UIP vaccine schedule from a baby's date of birth.
using MaatriCare.Models;
namespace MaatriCare.Services;

public static class VaccineService
{
    public static List<Vaccination> Schedule(DateTime dob)
    {
        var plan = new (string Name, int Days)[]
        {
            ("BCG", 0), ("OPV 0", 0), ("Hepatitis B (birth)", 0),
            ("Pentavalent 1 + Rota 1 + IPV 1", 42),
            ("Pentavalent 2 + Rota 2", 70),
            ("Pentavalent 3 + Rota 3 + IPV 2", 98),
            ("MR 1", 270)
        };
        return plan.Select(p => new Vaccination { Name = p.Name, DueDate = dob.AddDays(p.Days) }).ToList();
    }
}
