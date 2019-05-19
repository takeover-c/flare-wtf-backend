using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Flare.Backend.Utils {
    public class StreamWithProgress : Stream {
        public long bytesTotal = 0;
        private Stream innerStream;

        public override bool CanRead
        {
            get { return this.innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return this.innerStream.Length; }
        }

        public override long Position
        {
            get { return this.innerStream.Position; }
            set { this.innerStream.Position = value; }
        }

        public StreamWithProgress(Stream stream)
        {
            this.innerStream = stream ?? throw new ArgumentNullException("stream");
        }

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return this.innerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.innerStream.Write(buffer, offset, count);
            this.ReportProgress(count);
        }

        public override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override async Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            await this.innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            this.ReportProgress(count);
        }

        public override void WriteByte(byte value)
        {
            this.innerStream.WriteByte(value);
            this.ReportProgress(1);
        }

        protected override void Dispose(bool disposing)
        {
            
        }

        private void ReportProgress(int count)
        {
            this.bytesTotal += count;
        }
    }
}