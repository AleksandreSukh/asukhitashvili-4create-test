using FluentMigrator;

namespace Test._4Create.Data.Migrations;

[TimestampedMigration(2025, 01, 27, 12, 58)]
public class InitializeDb : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table(DbConstants.Tables.ClinicalTrialMetadatas)
            .WithColumn("TrialId").AsAnsiString().PrimaryKey()
            .WithColumn("Title").AsString(500).NotNullable()
            .WithColumn("StartDate").AsDate().NotNullable()
            .WithColumn("EndDate").AsDate().Nullable()
            .WithColumn("Participants").AsInt32().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable()
            .WithColumn("DurationDays").AsInt32().Nullable();
    }
}