using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Vhd;

namespace Stardust.Hdd
{
    public class Class1
    {
        public static void Genhdd()
        {
            long diskSize = 30 * 1024 * 1024; //30MB
            using (Stream vhdStream = File.Create("mydisk.vhd"))
            {
                Disk disk = Disk.InitializeDynamic(vhdStream, ownsStream: DiscUtils.Streams.Ownership.None, diskSize);
                BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsFat);
                using (FatFileSystem fs = FatFileSystem.FormatPartition(disk, 0, null))
                {
                    fs.CreateDirectory(@"TestDir\CHILD");
                    // do other things with the file system...
                }
            }
        }
    }
}
