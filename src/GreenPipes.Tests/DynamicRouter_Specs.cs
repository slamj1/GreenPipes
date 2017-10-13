﻿// Copyright 2012-2016 Chris Patterson
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
namespace GreenPipes.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Filters;
    using NUnit.Framework;
    using Pipes;


    public class DynamicRouter_Specs
    {
        bool _aWasCalled;
        bool _bWasCalled;
        IDynamicRouter<VendorContext> _router;

        [SetUp]
        public void SetUp()
        {
            _aWasCalled = false;
            _bWasCalled = false;

            _router = new DynamicRouter<VendorContext>(new VendorConverterFactory());

            IPipe<VendorContext<VendorARecord>> pipeA = Pipe.New<VendorContext<VendorARecord>>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    _aWasCalled = true;
                });
            });
            IPipe<VendorContext<VendorBRecord>> pipeB = Pipe.New<VendorContext<VendorBRecord>>(cfg =>
            {
                cfg.UseExecute(cxt =>
                {
                    _bWasCalled = true;
                });
            });

            _router.ConnectPipe(pipeA);
            _router.ConnectPipe(pipeB);


            Console.WriteLine(_router.GetProbeResult().ToJsonString());
        }

        [Test]
        public async Task A()
        {
            await _router.Send(new Vendor<VendorARecord>("A"));

            Assert.That(_aWasCalled, Is.True);
            Assert.That(_bWasCalled, Is.False);
        }

        [Test]
        public async Task B()
        {
            await _router.Send(new Vendor<VendorBRecord>("B"));

            Assert.That(_aWasCalled, Is.False);
            Assert.That(_bWasCalled, Is.True);
        }


        public class VendorConverterFactory : IPipeContextConverterFactory<VendorContext>
        {
            public IPipeContextConverter<VendorContext, TOutput> GetConverter<TOutput>()
                where TOutput : class, PipeContext
            {
                //Look at the output context type (which should come from this base interface)
                //find the generic argument, then make a converter with the right closed type.
                var innerType = typeof(TOutput).GetClosingArguments(typeof(VendorContext<>)).Single();

                return
                    (IPipeContextConverter<VendorContext, TOutput>)
                        Activator.CreateInstance(typeof(Converter<>).MakeGenericType(innerType));
            }


            class Converter<TVendor> :
                IPipeContextConverter<VendorContext, VendorContext<TVendor>>
                where TVendor : class, VendorRecord
            {
                bool IPipeContextConverter<VendorContext, VendorContext<TVendor>>.TryConvert(VendorContext input,
                    out VendorContext<TVendor> output)
                {
                    //now that we have the correct TVendor, we simply cast the input to the right
                    //type, in a more complex scenario we would use dependencies to build
                    //and bind data
                    output = input as VendorContext<TVendor>;

                    return output != null;
                }
            }
        }


        public interface VendorContext : PipeContext
        {
            string RawData { get; }
        }


        public interface VendorContext<TVendorRecord> : VendorContext
            where TVendorRecord : VendorRecord
        {
            TVendorRecord Record { get; }
        }


        public class Vendor : BasePipeContext,
            VendorContext
        {
            public Vendor(string rawData)
            {
                RawData = rawData;
            }

            public string RawData { get; set; }
        }


        public class Vendor<TVendorRecord> : BasePipeContext,
            VendorContext<TVendorRecord>
            where TVendorRecord : VendorRecord
        {
            public Vendor(string rawData)
            {
                RawData = rawData;
            }

            public string RawData { get; set; }
            public TVendorRecord Record { get; set; }
        }


        public interface VendorRecord
        {
        }


        public class VendorARecord : VendorRecord
        {
        }


        public class VendorBRecord : VendorRecord
        {
        }
    }
}