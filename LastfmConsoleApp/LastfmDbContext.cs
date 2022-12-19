using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LastfmConsoleApp;

public class LastfmDbContext : DbContext
{
    public DbSet<LastTrackDto> Scrobbles { get; set; }

    public DbSet<LastAlbumDto> Albums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LastAlbumDto>()
            .Property(p => p.ReleaseDate)
            .HasColumnType("date");
        modelBuilder.Entity<LastAlbumDto>()
            .Property(p => p.Url)
            .IsRequired(false);

        modelBuilder.Entity<LastTrackDto>()
            .Property(p => p.ArtistMbid)
            .IsRequired(false);
        modelBuilder.Entity<LastTrackDto>()
            .Property(p => p.AlbumMbid)
            .IsRequired(false);

        base.OnModelCreating(modelBuilder);
    }
}

[Table("scrobbles")]
public class LastTrackDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Mbid { get; set; }

    public string ArtistName { get; set; }

    public string ArtistMbid { get; set; }

    public Uri Url { get; set; }

    // public LastImageSet Images { get; set; }

    public string AlbumName { get; set; }

    public string AlbumMbid { get; set; }

    // public IEnumerable<LastTag> TopTags { get; set; }

    public DateTimeOffset? TimePlayed { get; set; }
}

public class LastAlbumDto
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string ArtistName { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int? Year { get; set; }

    public string Mbid { get; set; }

    public Uri Url { get; set; }
}
