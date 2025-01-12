using GenealogyTree.BLL.Entities;

namespace GenealogyTree.BLL.Interfaces
{
    public interface IFamilyTree
    {
        List<Person> People { get; set; }

        void AddPerson(Person person);

        Person? FindPerson(Guid id);

        void SetRelation(Guid fromId, Guid toId, string relation);

        List<Person> GetRelatives(Guid id, string relation);

        string PrintTree();

        (int Years, int Months, int Days) DetermineAgeAtBirth(Guid fromId, Guid toId);

        Person FindCommonAncestors(Guid personId1, Guid personId2);
    }
}
