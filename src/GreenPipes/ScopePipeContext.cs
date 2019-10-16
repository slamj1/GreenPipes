// Copyright 2012-2019 Chris Patterson
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
namespace GreenPipes
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Payloads;


    public class ScopePipeContext
    {
        readonly PipeContext _context;
        IPayloadCache _payloadCache;

        /// <summary>
        /// A pipe using the parent scope cancellationToken
        /// </summary>
        /// <param name="context"></param>
        protected ScopePipeContext(PipeContext context)
        {
            _context = context;
        }

        /// <summary>
        /// A pipe using the parent scope cancellationToken
        /// </summary>
        /// <param name="context"></param>
        /// <param name="payloads">Loads the payload cache with the specified objects</param>
        protected ScopePipeContext(PipeContext context, params object[] payloads)
        {
            _context = context;

            if (payloads != null && payloads.Length > 0)
                _payloadCache = new ListPayloadCache(payloads);
        }

        public virtual CancellationToken CancellationToken => _context.CancellationToken;

        IPayloadCache PayloadCache
        {
            get
            {
                if (_payloadCache != null)
                    return _payloadCache;

                while (Volatile.Read(ref _payloadCache) == null)
                    Interlocked.CompareExchange(ref _payloadCache, new ListPayloadCache(), null);

                return _payloadCache;
            }
        }

        public virtual bool HasPayloadType(Type payloadType)
        {
            return payloadType.GetTypeInfo().IsInstanceOfType(this) || PayloadCache.HasPayloadType(payloadType) || _context.HasPayloadType(payloadType);
        }

        public virtual bool TryGetPayload<T>(out T payload)
            where T : class
        {
            if (this is T context)
            {
                payload = context;
                return true;
            }

            return PayloadCache.TryGetPayload(out payload) || _context.TryGetPayload(out payload);
        }

        public virtual T GetOrAddPayload<T>(PayloadFactory<T> payloadFactory)
            where T : class
        {
            if (this is T context)
                return context;

            if (PayloadCache.TryGetPayload<T>(out var payload))
                return payload;

            if (_context.TryGetPayload(out payload))
                return payload;

            return PayloadCache.GetOrAddPayload(payloadFactory);
        }

        public virtual T AddOrUpdatePayload<T>(PayloadFactory<T> addFactory, UpdatePayloadFactory<T> updateFactory)
            where T : class
        {
            if (this is T context)
                return context;

            if (PayloadCache.TryGetPayload<T>(out var payload))
                return PayloadCache.AddOrUpdatePayload(addFactory, updateFactory);

            if (_context.TryGetPayload(out payload))
            {
                T Add() => updateFactory(payload);

                return PayloadCache.AddOrUpdatePayload(Add, updateFactory);
            }

            return PayloadCache.AddOrUpdatePayload(addFactory, updateFactory);
        }
    }
}
