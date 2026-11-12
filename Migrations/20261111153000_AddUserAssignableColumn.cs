using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCLogAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAssignableColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('User', 'Assignable') IS NULL
BEGIN
    ALTER TABLE [User]
    ADD [Assignable] bit NOT NULL
    CONSTRAINT [DF_User_Assignable] DEFAULT (0);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('User', 'Assignable') IS NOT NULL
BEGIN
    DECLARE @ConstraintName nvarchar(128);

    SELECT @ConstraintName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'User' AND c.name = 'Assignable';

    IF @ConstraintName IS NOT NULL
        EXEC('ALTER TABLE [User] DROP CONSTRAINT [' + @ConstraintName + ']');

    ALTER TABLE [User] DROP COLUMN [Assignable];
END
");
        }
    }
}
