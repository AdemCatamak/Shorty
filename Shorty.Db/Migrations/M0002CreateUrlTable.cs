using FluentMigrator;

namespace Shorty.Db.Migrations;

[Migration(0002)]
public class M0002CreateUrlTable : Migration
{
    public override void Up()
    {
        Create.Table("Urls")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("OriginalUrl").AsString(1024).NotNullable()
            .WithColumn("ShortUrl").AsString(1024).NotNullable()
            .WithColumn("UrlGroupId").AsGuid().NotNullable()
            ;

        Create.Index("IX_UrlGroups_ShortenedUrl_UrlGroupId")
            .OnTable("Urls")
            .OnColumn("ShortUrl").Ascending()
            .OnColumn("UrlGroupId").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("Urls");
    }
}