﻿using System;
using InfluxDB.Collector.Pipeline;
using InfluxDB.Collector.Pipeline.Batch;

namespace InfluxDB.Collector.Configuration
{
    class PipelinedCollectorBatchConfiguration : CollectorBatchConfiguration
    {
        readonly CollectorConfiguration _configuration;
        TimeSpan? _interval;
        int? _maxBatchSize;
        int? _workers;

        public PipelinedCollectorBatchConfiguration(CollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public override CollectorConfiguration AtInterval(TimeSpan interval, int? maxBatchSize, int? workers)
        {
            _workers = workers;
            _interval = interval;
            _maxBatchSize = maxBatchSize;
            return _configuration;
        }

        public IPointEmitter CreateEmitter(IPointEmitter parent, out Action dispose)
        {
            if (_interval == null)
            {
                dispose = null;
                return parent;
            }

            var batcher = new IntervalBatcher(_interval.Value, _maxBatchSize, _workers, parent);
            dispose = batcher.Dispose;
            return batcher;
        }
    }
}