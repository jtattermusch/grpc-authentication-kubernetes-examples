// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

using JWT;
using JWT.Builder;
using JWT.Algorithms;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            // The JWT check would usually be done by an interceptor, but for simplicity we check directly
            // in the handler.
            var jwtToken = GetAuthBearerToken(context.RequestHeaders);
            string authenticatedUser = GetAuthenticatedUserFromJwt(jwtToken);

            string authMsg = "";
            if (authenticatedUser != null)
            {
                authMsg = " (authenticated as \"" + authenticatedUser + "\" via JWT)";
            }
            string mtlsPeerIdentity = context.AuthContext.PeerIdentity.FirstOrDefault()?.Value;
            if (mtlsPeerIdentity != null)
            {
               authMsg = " (authenticated as \"" + mtlsPeerIdentity + "\" via mTLS)";
            }

            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name + authMsg });
        }

        // extracts bearer token from request's initial metadata
        static string GetAuthBearerToken(Metadata requestHeaders)
        {
            var authToken = requestHeaders.FirstOrDefault((entry) => entry.Key == "authorization")?.Value;

            if (authToken == null)
            {
                return null;
            }

            var parts = authToken.Split(" ", 2);
            if (parts.Length == 2 && parts[0] == "Bearer")
            {
                return parts[1];
                
            }
            return null;
        }

        // verify JWT signature and contents
        static string GetAuthenticatedUserFromJwt(string jwtToken)
        {
            if (jwtToken == null)
            {
                return null;
            }

            try
            {
                var json = new JwtBuilder()
                    // secret good for testing only!
                    .WithSecret("GrpcAuthDemoTestOnlySecret12345")
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(jwtToken);

                // check this jwt is for the right service
                if (json["aud"].ToString() != "helloworld.Greeter")
                {
                    return null;
                }
                
                string authenticatedSubject = json["sub"].ToString();
                return authenticatedSubject;
            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
            }
            catch (Exception)
            {
                Console.WriteLine("Error verifying token");
            }

            return null;
        }
    }
}
