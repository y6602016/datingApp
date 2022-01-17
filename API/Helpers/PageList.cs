
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
  public class PageList<T> : List<T>
  {
    public PageList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
      CurrentPage = pageNumber;
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      PageSize = pageSize;
      TotalCount = count;
      AddRange(items);
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    // a static method can be called anywhere
    public static async Task<PageList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
      // make a db call to count the total count
      var count = await source.CountAsync();
      // all objects of pageNumber = skip ((pageNumber - 1) * pageSize) objects then take (pageSize) objects 
      // ex: pageNumber = 1, pageSize = 5, we skip((1 - 1) * 5), then take (5) objects, which is 1th-5th
      // pageNumber = 2, pageSize = 5, we skip((2 - 1) * 5) = skip 1th-5th, then take (5) objects, which is 6th-10th
      var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

      // create and return a new PageList object, call the constructor
      return new PageList<T>(items, count, pageNumber, pageSize);
    }
  }
}