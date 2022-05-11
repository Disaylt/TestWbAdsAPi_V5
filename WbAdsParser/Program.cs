using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

while(true)
{
    string? key = Console.ReadLine();
    string? xInfo = await GetXInfo();
    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(xInfo))
    {
        var adsProducts = await GetAdsProducts(key);
        if (adsProducts.Adverts.Count > 0)
        {
            string stringProsucts = ConvertProdcutsToString(adsProducts);
            var products = await GetProducts(xInfo, stringProsucts);
            
        }
        else
        {
            Console.WriteLine("Не получил данные о рекламе");
        }
    }
    else
    {
        Console.WriteLine("Ключ либо xInfo пуст");
    }
}

IEnumerable<Advert> GetFirstSubjectProducts(RootAds adsProducts)
{
    var advers = adsProducts.Adverts
        .Where(x => x.Subject == adsProducts.PrioritySubjects[0]);
    return advers;
}

int GetMinTime(List<SearchProduct> products, IEnumerable<Advert> firstSubjectAdverts)
{
    int minNum = default;
    foreach(var firstSubProduct in firstSubjectAdverts)
    {
        int time = products.FirstOrDefault(x => x.id == firstSubProduct.id)?.time2 ?? 0;
        if(time > minNum)
        {
            minNum = time;
        }
    }
    if(minNum < 21)
    {
        minNum = 21;
    }
    return minNum;
}

async Task<RootAds> GetAdsProducts(string key)
{
    RootAds products = new RootAds();
    using (var httpClient = new HttpClient())
    {
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://catalog-ads.wildberries.ru/api/v5/search?keyword={key}"))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.wildberries.ru");
            request.Headers.TryAddWithoutValidation("DNT", "1");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");

            var response = await httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            products = JToken.Parse(responseText)?.ToObject<RootAds>() ?? new RootAds();
        }
    }
    return products;
}

async Task<List<SearchProduct>> GetProducts(string xInfo, string productsId)
{
    List<SearchProduct> products = new List<SearchProduct>();
    using (var httpClient = new HttpClient())
    {
        using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://wbxcatalog-ru.wildberries.ru/nm-2-card/products?{xInfo}&nm={productsId}"))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.wildberries.ru/catalog/0/search.aspx?search=%D0%B1%D0%B0%D0%B4");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.wildberries.ru");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");

            var response = await httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            products = JToken.Parse(responseText)["data"]?["products"]?.ToObject<List<SearchProduct>>() ?? new List<SearchProduct>();
        }
    }
    return products;
}

async Task<string?> GetXInfo()
{
    var handler = new HttpClientHandler();
    handler.UseCookies = false;
    string? xInfo = string.Empty;
    using (var httpClient = new HttpClient(handler))
    {
        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.wildberries.ru/user/get-xinfo-v2"))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");
            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "UTF-8");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.wildberries.ru/");
            request.Headers.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
            request.Headers.TryAddWithoutValidation("x-spa-version", "9.2.13");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.wildberries.ru");
            request.Headers.TryAddWithoutValidation("DNT", "1");
            request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            request.Headers.TryAddWithoutValidation("Content-Length", "0");
            request.Headers.TryAddWithoutValidation("TE", "trailers");
            
            var response = await httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();
            xInfo = JToken.Parse(responseText).Value<string>("xinfo");
        }
    }
    return xInfo;
}

string ConvertProdcutsToString(RootAds products)
{
    string productsString = string.Empty;
    foreach (var product in products.Adverts)
    {
        productsString += $"{product.id};";
    }
    return productsString;
}

public class Product
{
    [JsonProperty("id")]
    public int id { get; set; }
}

public class SearchProduct : Product
{
    public int time2 { get; set; }
}

public class Advert : Product
{
    [JsonProperty("advertId")]
    public int AdvertId { get; set; }

    [JsonProperty("cpm")]
    public int Cpm { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("subject")]
    public int Subject { get; set; }
}

public class Page
{
    [JsonProperty("page")]
    public int NumPage { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("positions")]
    public List<int> Positions { get; set; }
}

public class RootAds
{
    [JsonProperty("pages")]
    public List<Page> Pages { get; set; }

    [JsonProperty("prioritySubjects")]
    public List<int> PrioritySubjects { get; set; }

    [JsonProperty("adverts")]
    public List<Advert> Adverts { get; set; }
}

