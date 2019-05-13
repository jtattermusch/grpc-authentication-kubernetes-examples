#!/bin/sh
# Copyright 2019 The gRPC Authors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# store certificates and keys as kubernetes secrets
kubectl create secret generic greeter-server-certs --from-file=../certs/server.pem --from-file=../certs/server.key --from-file=../certs/ca.pem
kubectl create secret generic greeter-client-certs --from-file=../certs/client.pem --from-file=../certs/client.key --from-file=../certs/ca.pem

