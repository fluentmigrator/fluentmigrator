namespace Adventure.Core.Maintenance
{
    // Root-module maintenance: runs once per database target
    // (BeforeAll/AfterAll around the module passes), not once per module.
    public class RebuildAllIndexes { }
}
