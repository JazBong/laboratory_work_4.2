using GenealogyTree.BLL.Entities;
using GenealogyTree.BLL.Interfaces;
using System.Text;

namespace GenealogyTree.BLL.Services
{
    public class FamilyTree : IFamilyTree
    {
        public List<Person> People { get; set; } = new();

        public void AddPerson(Person person) => People.Add(person);

        public Person? FindPerson(Guid id) => People.FirstOrDefault(p => p.Id == id);
        public static bool HasRelation(Person person1, Person person2, string relation)
        {
            if (person2.Relations.ContainsKey(relation))
            {
                return person2.Relations[relation].Contains(person1.Id);
            }
            return false;
        }

        public void SetRelation(Guid fromId, Guid toId, string relation)
        {
            var from = FindPerson(fromId);
            var to = FindPerson(toId);
            if (from == null || to == null || from == to || !(relation == "parent" || relation == "child" || relation == "spouse"))
            {
                throw new Exception("Incorrect data");
            }

            if (!to.Relations.ContainsKey(relation))
            {
                to.Relations[relation] = new List<Guid>();
            }
            else if (HasRelation(from, to, relation))
            {
                return;
            }

            to.Relations[relation].Add(fromId);

            if (relation == "spouse")
            {
                if (!HasRelation(to, from, "spouse"))
                {
                    SetRelation(toId, fromId, "spouse");
                }
                var childs = GetRelatives(fromId, "child");
                foreach (var ch in childs)
                {
                    SetRelation(toId, ch.Id, "parent");
                }
            }
            else if (relation == "parent")
            {
                if (!HasRelation(to, from, "child"))
                {
                    SetRelation(toId, fromId, "child");
                }
                var spouses = GetRelatives(fromId, "spouse");
                foreach (var sp in spouses)
                {
                    SetRelation(toId, sp.Id, "child");
                }
            }
            else if (relation == "child")
            {
                if (!HasRelation(to, from, "parent"))
                {
                    SetRelation(toId, fromId, "parent");
                }
                var spouses = GetRelatives(toId, "spouse");
                foreach (var sp in spouses)
                {
                    SetRelation(fromId, sp.Id, "child");
                }
            }
        }

        public List<Person> GetRelatives(Guid id, string relation)
        {
            var person = FindPerson(id);
            if (person == null || !person.Relations.ContainsKey(relation)) return new List<Person>();

            return person.Relations[relation].Select(FindPerson).Where(p => p != null).Cast<Person>().ToList();
        }

        public string PrintTree()
        {
            StringBuilder data = new StringBuilder();
            foreach (var person in People)
            {
                data.AppendLine();
                data.AppendLine(person.ToString());
                foreach (var relation in person.Relations)
                {
                    data.AppendLine($"  {relation.Key}: {string.Join(", ", relation.Value.Select(id => FindPerson(id)?.FullName ?? "Unknown"))}");
                }
            }
            return data.ToString();
        }

        public (int Years, int Months, int Days) DetermineAgeAtBirth(Guid fromId, Guid toId)
        {
            var from = FindPerson(fromId);
            var to = FindPerson(toId);
            if (from == null || to == null)
            {
                throw new Exception("Incorrect data");
            }

            if (from.BirthDate > to.BirthDate)
            {
                (from, to) = (to, from);
            }
            int years = to.BirthDate.Year - from.BirthDate.Year;
            int months = to.BirthDate.Month - from.BirthDate.Month;
            int days = to.BirthDate.Day - from.BirthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(to.BirthDate.Year, to.BirthDate.Month == 1 ? 12 : to.BirthDate.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return (years, months, days);

        }

        public Person FindCommonAncestors(Guid personId1, Guid personId2)
        {
            var ancestors1 = GetAllAncestors(personId1);
            var ancestors2 = GetAllAncestors(personId2);

            foreach (var anc in ancestors1)
            {
                if (ancestors2.Contains(anc))
                {
                    return anc;
                }
            }
            return null;
        }
        private List<Person> GetAllAncestors(Guid personId)
        {
            var ancestors = new List<Person>();
            var parents = GetRelatives(personId, "parent");
            ancestors.AddRange(parents);
            foreach (var parent in parents)
            {
                ancestors.AddRange(GetAllAncestors(parent.Id));
            }

            return ancestors;
        }

    }
}
