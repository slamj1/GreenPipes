// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading.Tasks;


    public interface IPipe<in TContext> :
        IProbeSite
        where TContext : class, PipeContext
    {
        /// <summary>
        /// The base primitive, Send delivers the pipe context of T to the pipe.
        /// </summary>
        /// <param name="context">The pipe context of type T</param>
        /// <returns>A task which is completed once the pipe has processed the context</returns>
        Task Send(TContext context);
    }
}