# Possible future design
![image](https://user-images.githubusercontent.com/26905747/236047314-9086da53-b54f-4632-ba82-4a10edcb3ce3.png)

#### crawlers
Set of jobs (k8s Cron Jobs or lambdas or .net services + hangfire). Gets some raw data using configuration from config service (resource, credentials?, params, metadata) and send it to Kafka topics (also read from config)
*Additional functionality:* send telemetry events on start, before stop, on faailure, on success. Those events used to build monitoring dashboard.

#### Data Warehouse
Master data storage. Consumers (writers) normalize, deduplicate, validate data and write it to appropriate transactional db (e.g. Postgre SQL). Query API available on the top of read replicas. Should write consistent data to storage (master) and fanout notifications that something updated. Exposes read API 
*Additional functionality:* provide some internal API and/or additional services to seed, translate, edit data. 
*Storage Scaling Options*: logical by sport and season (split current season and historical data), possible partitioning by competitions, vertical, horizontal with readonly replicas

#### Data Sinks
Public API for data. Exposes HTTP API (+GrapQL?) and WebSocets(for live events). API is cached (Expirational and where possible ETag). Subscribed to the changes from Data Warehouse. Stores denormolized copies of master data in some document db (e.g. Mongo) and exposes API on the top of it.
Most used projections (e.g. standings) could be extarcted to separate services (to store and scale them separately). Historical data or data for queries that have too many filters could be stored in ellasticsearch. 
*Storage Scaling Options*: logical by sport and season (split current season and historical data), sharding by competitions, vertical and horizontal (with write concern - majority)
