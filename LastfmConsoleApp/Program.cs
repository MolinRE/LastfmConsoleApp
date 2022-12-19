using AutoMapper;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using LastfmConsoleApp;
using Microsoft.EntityFrameworkCore;

var client = new LastfmClient(Environment.GetEnvironmentVariable("LASTFM_API_KEY"), Environment.GetEnvironmentVariable("LASTFM_API_SECRET"));
var config = new MapperConfiguration(cfg => cfg.CreateMap<LastTrack, LastTrackDto>());
var mapper = config.CreateMapper();

// TODO: mbid null
// TODO: После добавления новых скробблингов добавить новые альбомы
// Во время добавления альбомов чистить mater title (deluxe edition и т.д.)

await using var db = new LastfmDbContext();

// db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var userName = "MolinRE";
var offset = await db.Scrobbles.MaxAsync(p => p.TimePlayed);
var pageNumber = 1;
var count = 200;
Console.WriteLine($"GetRecentScrobbles: userName = {userName}, offset = {offset}, pageNumber = {pageNumber}, count = {count}");
var scrobbles = await Load(pageNumber);
var albums = new List<LastAlbumDto>();
FillNewAlbums(albums, scrobbles.Content);

Add(scrobbles.Content);

while (pageNumber < scrobbles.TotalPages)
{
    pageNumber++;
    scrobbles = await Load(pageNumber);
    Add(scrobbles.Content);
    FillNewAlbums(albums, scrobbles.Content);
}

var old = await db.Albums.ToArrayAsync();
await db.Albums.AddRangeAsync(albums.Where(p1 => !Contains(old, p1)));

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

void FillNewAlbums(List<LastAlbumDto> albums, IReadOnlyList<LastTrack> scrobbles)
{
    albums.AddRange(scrobbles.Select(s => new LastAlbumDto()
    {
        ArtistName = s.ArtistName,
        Title = s.AlbumName,
        Mbid = s.AlbumMbid
    }).Where(p1 => !Contains(albums, p1)));
}

bool Contains(IEnumerable<LastAlbumDto> collection, LastAlbumDto a)
{
    return collection.Any(p => p.ArtistName == a.ArtistName && p.Title == a.Title);
}

