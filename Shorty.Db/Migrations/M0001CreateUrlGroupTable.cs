using FluentMigrator;

namespace Shorty.Db.Migrations;

[Migration(0001)]
public class M0001CreateUrlGroupTable : Migration
{
    public override void Up()
    {
        Create.Table("UrlGroups")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("Name").AsString(10).NotNullable()
            .WithColumn("Description").AsString(int.MaxValue).Nullable()
            .WithColumn("OwnerId").AsString(64).NotNullable()
            ;

        Create.Index("IX_UrlGroups_Name")
            .OnTable("UrlGroups")
            .OnColumn("Name").Ascending().WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("UrlGroups");
    }
}