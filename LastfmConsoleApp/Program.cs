using AutoMapper;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using LastfmConsoleApp;

var client = new LastfmClient(Environment.GetEnvironmentVariable("LASTFM_API_KEY"), Environment.GetEnvironmentVariable("LASTFM_API_SECRET"));
var config = new MapperConfiguration(cfg => cfg.CreateMap<LastTrack, LastTrackDto>());
var mapper = config.CreateMapper();


var userName = "MolinRE";
var offset = new DateTime(2021, 12, 1);
var pageNumber = 1;
var count = 200;
Console.WriteLine($"GetRecentScrobbles: userName = {userName}, offset = {offset}, pageNumber = {pageNumber}, count = {count}");
var scrobbles = await Load(pageNumber);

// TODO: mbid null
// TODO: После добавления новых скробблингов добавить новые альбомы
// Во время добавления альбомов чистить mater title (deluxe edition и т.д.)

await using var db = new LastfmDbContext();

// db.Database.EnsureDeleted();
db.Database.EnsureCreated();

Console.WriteLine("На данный момент код умеет только загружать скробблинги с определенной даты.");

return;

Add(scrobbles.Content);

while (pageNumber < scrobbles.TotalPages)
{
    pageNumber++;
    scrobbles = await Load(pageNumber);
    Add(scrobbles.Content);
}

Console.WriteLine("Сохранение скробблингов в БД...");
await db.SaveChangesAsync();
Console.WriteLine("Готово");

void Add(IReadOnlyList<LastTrack> content)
{
    foreach (var scrobble in content)
    {
        var dto = mapper.Map<LastTrackDto>(scrobble);
        db.Scrobbles.Add(dto);
    }
}

async Task<PageResponse<LastTrack>> Load(int page)
{
    var scrobbles = await client.User.GetRecentScrobbles(userName, offset, page, count);
    Console.WriteLine($"Страница {scrobbles.Page}/{scrobbles.TotalPages}, размер = {scrobbles.PageSize}");

    return scrobbles;
}

