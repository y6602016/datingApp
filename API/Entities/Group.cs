
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
  public class Group
  {
    public Group()
    {
    }

    public Group(string name)
    {
      Name = name;
    }

    // make the name as the key of the group table
    [Key]
    public string Name { get; set; }
    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
  }
}