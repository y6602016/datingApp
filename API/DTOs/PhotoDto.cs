namespace API.DTOs
{
  public class PhotoDto
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }
    // not referencing to user object such that there is no cycle
  }
}