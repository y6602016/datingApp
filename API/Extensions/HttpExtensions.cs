

using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
  public static class HttpExtensions
  {
    public static void AddPaginationHeader(this HttpResponse response, int currentPage,
    int itemsPerPage, int totalItems, int totalPages)
    {
      // create the pagination header
      var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

      // add the header into the response header, Add() needs "stringValue" as value, so serialize to Json
      response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader));

      // add the header we create to "Access-Control-Expose-Headers"
      response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }

  }
}