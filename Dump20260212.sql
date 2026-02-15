-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: isdn_distribution_db
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260210145318_InitialCreate','8.0.23');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audit_logs`
--

DROP TABLE IF EXISTS `audit_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_logs` (
  `audit_id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `action` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `entity_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `entity_id` int DEFAULT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ip_address` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `created_at` datetime(6) NOT NULL,
  PRIMARY KEY (`audit_id`),
  KEY `IX_audit_logs_user_id` (`user_id`),
  CONSTRAINT `FK_audit_logs_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_logs`
--

LOCK TABLES `audit_logs` WRITE;
/*!40000 ALTER TABLE `audit_logs` DISABLE KEYS */;
INSERT INTO `audit_logs` VALUES (1,1,'ADMIN_CREATED','User',1,'Default admin user created','System','2026-02-10 14:54:31.218360'),(2,29,'LOGIN_SUCCESS','User',29,'User logged in successfully','::1','2026-02-10 15:00:36.485979'),(3,29,'LOGOUT','User',29,'User logged out','::1','2026-02-10 15:00:46.391787'),(4,20,'LOGIN_SUCCESS','User',20,'User logged in successfully','::1','2026-02-10 15:01:44.743875'),(5,20,'LOGOUT','User',20,'User logged out','::1','2026-02-10 15:01:51.235457'),(6,15,'LOGIN_SUCCESS','User',15,'User logged in successfully','::1','2026-02-10 15:02:28.936235'),(7,15,'LOGOUT','User',15,'User logged out','::1','2026-02-10 16:37:13.790755'),(8,15,'LOGIN_SUCCESS','User',15,'User logged in successfully','::1','2026-02-10 16:37:21.357035'),(9,15,'LOGOUT','User',15,'User logged out','::1','2026-02-10 16:37:24.938939'),(10,18,'LOGIN_SUCCESS','User',18,'User logged in successfully','::1','2026-02-10 16:38:16.825734'),(11,1,'LOGIN_SUCCESS','User',1,'User logged in successfully','::1','2026-02-11 10:38:56.764353'),(12,1,'LOGOUT','User',1,'User logged out','::1','2026-02-11 10:40:58.029543'),(13,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 10:41:46.842106'),(14,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 10:42:24.621999'),(15,9,'LOGIN_SUCCESS','User',9,'User logged in successfully','::1','2026-02-11 10:43:20.485635'),(16,9,'LOGOUT','User',9,'User logged out','::1','2026-02-11 10:43:49.007573'),(17,10,'LOGIN_SUCCESS','User',10,'User logged in successfully','::1','2026-02-11 10:44:42.908589'),(18,10,'LOGOUT','User',10,'User logged out','::1','2026-02-11 10:45:20.670549'),(20,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 13:57:39.144576'),(21,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 14:28:17.533486'),(22,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 14:36:05.055676'),(23,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 15:13:22.808429'),(24,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 15:15:31.390276'),(25,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 15:19:45.025217'),(26,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 15:24:23.917467'),(27,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 16:47:28.294661'),(28,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 16:51:09.612564'),(29,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 17:19:58.658293'),(30,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 17:20:31.417202'),(31,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 17:40:33.849776'),(32,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 17:41:11.182454'),(33,2,'LOGIN_SUCCESS','User',2,'User logged in successfully','::1','2026-02-11 19:46:43.092950'),(34,2,'LOGOUT','User',2,'User logged out','::1','2026-02-11 21:08:21.074798'),(35,47,'LOGIN_SUCCESS','User',47,'User logged in successfully','::1','2026-02-11 21:10:57.476002'),(36,47,'LOGOUT','User',47,'User logged out','::1','2026-02-11 21:25:41.627809'),(37,47,'LOGIN_SUCCESS','User',47,'User logged in successfully','::1','2026-02-11 21:26:57.004062'),(38,47,'LOGIN_SUCCESS','User',47,'User logged in successfully','::1','2026-02-11 23:46:36.221952'),(39,47,'LOGOUT','User',47,'User logged out','::1','2026-02-12 01:37:03.314069'),(40,24,'LOGIN_SUCCESS','User',24,'User logged in successfully','::1','2026-02-12 01:40:20.680004'),(41,24,'LOGOUT','User',24,'User logged out','::1','2026-02-12 01:48:00.023804'),(42,47,'LOGIN_SUCCESS','User',47,'User logged in successfully','::1','2026-02-12 01:48:19.847068'),(43,47,'LOGOUT','User',47,'User logged out','::1','2026-02-12 02:17:28.322619'),(44,24,'LOGIN_SUCCESS','User',24,'User logged in successfully','::1','2026-02-12 02:18:11.412122');
/*!40000 ALTER TABLE `audit_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `customer_id` int NOT NULL AUTO_INCREMENT,
  `user_id` int DEFAULT NULL,
  `first_name` varchar(50) NOT NULL,
  `last_name` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `phone_number` varchar(20) NOT NULL,
  `business_name` varchar(150) NOT NULL,
  `street_address` varchar(255) NOT NULL,
  `city` varchar(100) NOT NULL,
  `zip_code` varchar(10) NOT NULL,
  `temp_password_hash` varchar(255) NOT NULL,
  `rdc_id` int DEFAULT NULL,
  `registration_status` enum('PENDING','APPROVED','DISAPPROVED') DEFAULT 'PENDING',
  `is_active` tinyint(1) DEFAULT '1',
  `disapproved_at` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`customer_id`),
  UNIQUE KEY `email` (`email`),
  KEY `fk_cust_user` (`user_id`),
  KEY `fk_cust_rdc` (`rdc_id`),
  CONSTRAINT `fk_cust_rdc` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`),
  CONSTRAINT `fk_cust_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customers`
--

LOCK TABLES `customers` WRITE;
/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
INSERT INTO `customers` VALUES (2,47,'Kamani','Perera','kamani@gmail.com','0771375643','ASD Enterprises','No 435, Colombo','Colombo','89678','$2a$11$vwbjF6tuMu3DUuDYGoAhcuTkuRAn3aDYB0WH4wLa5TEFKVF2PmvEy',4,'APPROVED',1,NULL,'2026-02-11 20:51:47'),(5,50,'Sudath','Perera','sudath@gmail.com','0713456789','sudath sellers','No 90, Colombo','Colombo','23456','$2a$11$SpFxJwRHIOpEV0o.GaEsXOlQR92K9mbSYUKCEu8S3j.C3RtQ7WU7a',4,'APPROVED',1,NULL,'2026-02-11 22:20:45'),(6,NULL,'Nuwan','Fernando','nuwan@gmail.com','0912234789','Nuwan Supermarket','No 345, Hambanthota','Hambanthota','34567','$2a$11$HMxO.u7txMP1vIMgok7nIO.wMjzPNWiV4EL3SoMbIEscsgVeK11Ru',NULL,'DISAPPROVED',0,'2026-02-12 01:31:03','2026-02-12 01:16:09');
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `deliveries`
--

DROP TABLE IF EXISTS `deliveries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `deliveries` (
  `delivery_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `rdc_id` int DEFAULT NULL,
  `driver_id` int DEFAULT NULL,
  `scheduled_date` datetime(6) DEFAULT NULL,
  `delivery_date` datetime(6) DEFAULT NULL,
  `status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `tracking_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `created_at` datetime(6) NOT NULL,
  PRIMARY KEY (`delivery_id`),
  KEY `IX_deliveries_driver_id` (`driver_id`),
  KEY `IX_deliveries_order_id` (`order_id`),
  KEY `IX_deliveries_rdc_id` (`rdc_id`),
  CONSTRAINT `FK_deliveries_orders_order_id` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE,
  CONSTRAINT `FK_deliveries_rdcs_rdc_id` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_deliveries_users_driver_id` FOREIGN KEY (`driver_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `deliveries`
--

LOCK TABLES `deliveries` WRITE;
/*!40000 ALTER TABLE `deliveries` DISABLE KEYS */;
/*!40000 ALTER TABLE `deliveries` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inventory`
--

DROP TABLE IF EXISTS `inventory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory` (
  `inventory_id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `rdc_id` int DEFAULT NULL,
  `location` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `quantity_available` int NOT NULL,
  `quantity_reserved` int NOT NULL,
  `reorder_level` int NOT NULL,
  `last_updated` datetime(6) NOT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`inventory_id`),
  KEY `IX_inventory_product_id` (`product_id`),
  KEY `IX_inventory_rdc_id` (`rdc_id`),
  CONSTRAINT `FK_inventory_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE,
  CONSTRAINT `FK_inventory_rdcs_rdc_id` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inventory`
--

LOCK TABLES `inventory` WRITE;
/*!40000 ALTER TABLE `inventory` DISABLE KEYS */;
INSERT INTO `inventory` VALUES (1,1,NULL,'Main Warehouse',1000,0,100,'2026-02-10 14:54:36.775033',1),(2,2,NULL,'Main Warehouse',1000,0,100,'2026-02-10 14:54:36.775087',1),(3,3,NULL,'Main Warehouse',1000,0,100,'2026-02-10 14:54:36.775087',1),(4,4,NULL,'Main Warehouse',1000,0,100,'2026-02-10 14:54:36.775087',1),(5,5,NULL,'Main Warehouse',1000,0,100,'2026-02-10 14:54:36.775087',1);
/*!40000 ALTER TABLE `inventory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `jwt_tokens`
--

DROP TABLE IF EXISTS `jwt_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `jwt_tokens` (
  `token_id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `token` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `expires_at` datetime(6) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `is_revoked` tinyint(1) NOT NULL,
  PRIMARY KEY (`token_id`),
  KEY `IX_jwt_tokens_user_id` (`user_id`),
  CONSTRAINT `FK_jwt_tokens_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `jwt_tokens`
--

LOCK TABLES `jwt_tokens` WRITE;
/*!40000 ALTER TABLE `jwt_tokens` DISABLE KEYS */;
INSERT INTO `jwt_tokens` VALUES (1,29,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMjkiLCJlbWFpbCI6ImNlbnRyYWwtcmRjQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6IkNlbnRyYWwgUkRDIFN0YWZmIiwicm9sZSI6IlJEQ19TVEFGRiIsInJvbGVfbmFtZSI6IlJEQ19TVEFGRiIsInJkY19pZCI6IjUiLCJuYmYiOjE3NzA3MzU2MzUsImV4cCI6MTc3MDc0MjgzNSwiaWF0IjoxNzcwNzM1NjM1LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.6m7dPO5nYkiNNvg0erhw4mCsZqfbFodCLbl6qP4-1QM','2026-02-10 17:00:35.515330','2026-02-10 15:00:35.515557',1),(2,20,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMjAiLCJlbWFpbCI6ImVhc3QtbG9naXN0aWNzQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6IkVhc3QgTG9naXN0aWNzIiwicm9sZSI6IkxPR0lTVElDUyIsInJvbGVfbmFtZSI6IkxPR0lTVElDUyIsInJkY19pZCI6IjMiLCJuYmYiOjE3NzA3MzU3MDQsImV4cCI6MTc3MDc0MjkwNCwiaWF0IjoxNzcwNzM1NzA0LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.cl7ZnOOzLIjEGDWsfxBmMv1E5Xhm6V05HELv2tk2hkw','2026-02-10 17:01:44.487494','2026-02-10 15:01:44.487495',1),(3,15,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMTUiLCJlbWFpbCI6InNvdXRoLWxvZ2lzdGljc0Bpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJTb3V0aCBMb2dpc3RpY3MiLCJyb2xlIjoiTE9HSVNUSUNTIiwicm9sZV9uYW1lIjoiTE9HSVNUSUNTIiwicmRjX2lkIjoiMiIsIm5iZiI6MTc3MDczNTc0OCwiZXhwIjoxNzcwNzQyOTQ4LCJpYXQiOjE3NzA3MzU3NDgsImlzcyI6IklTRE5EaXN0cmlidXRpb25TeXN0ZW0iLCJhdWQiOiJJU0ROVXNlcnMifQ.sTO8sfXAkAkluuUlTcGhSZtDT09dh9BlgnS-eqDDLsU','2026-02-10 17:02:28.433639','2026-02-10 15:02:28.433641',1),(4,15,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMTUiLCJlbWFpbCI6InNvdXRoLWxvZ2lzdGljc0Bpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJTb3V0aCBMb2dpc3RpY3MiLCJyb2xlIjoiTE9HSVNUSUNTIiwicm9sZV9uYW1lIjoiTE9HSVNUSUNTIiwicmRjX2lkIjoiMiIsIm5iZiI6MTc3MDc0MTQ0MCwiZXhwIjoxNzcwNzQ4NjQwLCJpYXQiOjE3NzA3NDE0NDAsImlzcyI6IklTRE5EaXN0cmlidXRpb25TeXN0ZW0iLCJhdWQiOiJJU0ROVXNlcnMifQ.mTgYgH_M3WhCPVFt9ivJHKGuky7l8LSCKtHnxiDolAo','2026-02-10 18:37:20.880435','2026-02-10 16:37:20.880503',1),(5,18,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMTgiLCJlbWFpbCI6InNvdXRoLXNhbGVzQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6IlNvdXRoIFNhbGVzIFJlcCIsInJvbGUiOiJTQUxFU19SRVAiLCJyb2xlX25hbWUiOiJTQUxFU19SRVAiLCJyZGNfaWQiOiIyIiwibmJmIjoxNzcwNzQxNDk2LCJleHAiOjE3NzA3NDg2OTYsImlhdCI6MTc3MDc0MTQ5NiwiaXNzIjoiSVNETkRpc3RyaWJ1dGlvblN5c3RlbSIsImF1ZCI6IklTRE5Vc2VycyJ9.6GQfkB8HG_OscTFl76lX9hhZbKnlnnAFqGSmT-n8b6s','2026-02-10 18:38:16.709067','2026-02-10 16:38:16.709068',0),(6,1,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMSIsImVtYWlsIjoiYWRtaW5AaXNkbi5sayIsInVuaXF1ZV9uYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJyb2xlIjoiQURNSU4iLCJyb2xlX25hbWUiOiJBRE1JTiIsIm5iZiI6MTc3MDgwNjMzNSwiZXhwIjoxNzcwODEzNTM1LCJpYXQiOjE3NzA4MDYzMzUsImlzcyI6IklTRE5EaXN0cmlidXRpb25TeXN0ZW0iLCJhdWQiOiJJU0ROVXNlcnMifQ.HzPmHFwNUttvwoYpiTwUIcPFi-GmeMG3JQD3XpYhJs8','2026-02-11 12:38:55.855458','2026-02-11 10:38:55.855516',1),(7,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MDY1MDYsImV4cCI6MTc3MDgxMzcwNiwiaWF0IjoxNzcwODA2NTA2LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.Vbci6z6y9TFHDe_obZxS_OPcB_ABmiBSmdB3W7TbOQE','2026-02-11 12:41:46.504957','2026-02-11 10:41:46.504958',1),(8,9,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiOSIsImVtYWlsIjoibm9ydGgtcmRjQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6Ik5vcnRoIFJEQyBTdGFmZiIsInJvbGUiOiJSRENfU1RBRkYiLCJyb2xlX25hbWUiOiJSRENfU1RBRkYiLCJyZGNfaWQiOiIxIiwibmJmIjoxNzcwODA2NjAwLCJleHAiOjE3NzA4MTM4MDAsImlhdCI6MTc3MDgwNjYwMCwiaXNzIjoiSVNETkRpc3RyaWJ1dGlvblN5c3RlbSIsImF1ZCI6IklTRE5Vc2VycyJ9.CRu4W78wSPOkt-ebFKqGcUw2S2P7u_Im2ZEPVB2kPZ8','2026-02-11 12:43:20.402066','2026-02-11 10:43:20.402066',1),(9,10,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMTAiLCJlbWFpbCI6Im5vcnRoLWxvZ2lzdGljc0Bpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJOb3J0aCBMb2dpc3RpY3MiLCJyb2xlIjoiTE9HSVNUSUNTIiwicm9sZV9uYW1lIjoiTE9HSVNUSUNTIiwicmRjX2lkIjoiMSIsIm5iZiI6MTc3MDgwNjY4MiwiZXhwIjoxNzcwODEzODgyLCJpYXQiOjE3NzA4MDY2ODIsImlzcyI6IklTRE5EaXN0cmlidXRpb25TeXN0ZW0iLCJhdWQiOiJJU0ROVXNlcnMifQ.Ejube6mRgFYP87VT1AY9BvIAgggO2frXtzlLoG2mkbg','2026-02-11 12:44:42.689270','2026-02-11 10:44:42.689271',1),(10,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MTgyNTgsImV4cCI6MTc3MDgyNTQ1OCwiaWF0IjoxNzcwODE4MjU4LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.Wrg1A_0hAe-wvWmtoPr4Y8HJt4DpYzcMo7aQhnVEh1I','2026-02-11 15:57:38.309898','2026-02-11 13:57:38.309950',1),(11,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MjA1NjQsImV4cCI6MTc3MDgyNzc2MywiaWF0IjoxNzcwODIwNTY0LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.atxVjQ0VVFOgSURvTPE5cEc6ePWIF6Sc0TspOr8Pzl0','2026-02-11 16:36:04.075571','2026-02-11 14:36:04.075653',1),(12,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MjI5MzEsImV4cCI6MTc3MDgzMDEzMSwiaWF0IjoxNzcwODIyOTMxLCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.YbVcNORI8h6VvJXKH00vTFxEgixVfUjCiwia014JcyY','2026-02-11 17:15:31.122881','2026-02-11 15:15:31.122938',1),(13,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MjM0NjMsImV4cCI6MTc3MDgzMDY2MywiaWF0IjoxNzcwODIzNDYzLCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.1HyJjmUx_6tcUgLefjkxHHOjmx9PLF7OGKRmHyoNP_0','2026-02-11 17:24:23.450570','2026-02-11 15:24:23.450571',1),(14,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4Mjg2NjgsImV4cCI6MTc3MDgzNTg2OCwiaWF0IjoxNzcwODI4NjY4LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.BCKpNJmnW8d1dry4qjyGlDUqC-zmnFv4O37eqmvVGPI','2026-02-11 18:51:09.229502','2026-02-11 16:51:09.229775',1),(15,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MzA0MzEsImV4cCI6MTc3MDgzNzYzMSwiaWF0IjoxNzcwODMwNDMxLCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.e-MmJyWiQ36X02xiMucTHildkry35VLs5USTr-Z9REY','2026-02-11 19:20:31.336519','2026-02-11 17:20:31.336814',1),(16,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MzE2NzAsImV4cCI6MTc3MDgzODg3MCwiaWF0IjoxNzcwODMxNjcwLCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.vVIkavuAwNMGMvRJafmgv-2LlnIcMwW0HR6yz2fdoxw','2026-02-11 19:41:10.520685','2026-02-11 17:41:10.520742',0),(17,2,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMiIsImVtYWlsIjoiaGVhZG9mZmljZUBpc2RuLmxrIiwidW5pcXVlX25hbWUiOiJIZWFkIE9mZmljZSBNYW5hZ2VyIiwicm9sZSI6IkhFQURfT0ZGSUNFIiwicm9sZV9uYW1lIjoiSEVBRF9PRkZJQ0UiLCJuYmYiOjE3NzA4MzkyMDEsImV4cCI6MTc3MDg0NjQwMSwiaWF0IjoxNzcwODM5MjAxLCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.DUupGWr4RFErf3WwVvgqPIJjH8jZOGXNGnWkLcOxbm0','2026-02-11 21:46:42.023339','2026-02-11 19:46:42.023391',1),(18,47,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNDciLCJlbWFpbCI6ImthbWFuaUBnbWFpbC5jb20iLCJ1bmlxdWVfbmFtZSI6IkthbWFuaSBQZXJlcmEiLCJyb2xlIjoiQ1VTVE9NRVIiLCJyb2xlX25hbWUiOiJDVVNUT01FUiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NDQyNTYsImV4cCI6MTc3MDg1MTQ1NiwiaWF0IjoxNzcwODQ0MjU2LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.J7hEagoIBCqg-X0y0GrUC4DX-rRp6LD-MiL1QEaGqs8','2026-02-11 23:10:57.100720','2026-02-11 21:10:57.100936',1),(19,47,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNDciLCJlbWFpbCI6ImthbWFuaUBnbWFpbC5jb20iLCJ1bmlxdWVfbmFtZSI6IkthbWFuaSBQZXJlcmEiLCJyb2xlIjoiQ1VTVE9NRVIiLCJyb2xlX25hbWUiOiJDVVNUT01FUiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NDUyMTUsImV4cCI6MTc3MDg1MjQxNSwiaWF0IjoxNzcwODQ1MjE1LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.ThGdWJJl21FGH1NSI839_SoEsODFB-HbzMa0gPobqQM','2026-02-11 23:26:56.044133','2026-02-11 21:26:56.044184',0),(20,47,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNDciLCJlbWFpbCI6ImthbWFuaUBnbWFpbC5jb20iLCJ1bmlxdWVfbmFtZSI6IkthbWFuaSBQZXJlcmEiLCJyb2xlIjoiQ1VTVE9NRVIiLCJyb2xlX25hbWUiOiJDVVNUT01FUiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NTM1OTQsImV4cCI6MTc3MDg2MDc5NCwiaWF0IjoxNzcwODUzNTk0LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.yRay_0aBXaMQ9-_JReWWWKvAYzRSsv3rRj_2QDxRkH0','2026-02-12 01:46:35.003076','2026-02-11 23:46:35.003126',1),(21,24,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMjQiLCJlbWFpbCI6Indlc3QtcmRjQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6Ildlc3QgUkRDIFN0YWZmIiwicm9sZSI6IlJEQ19TVEFGRiIsInJvbGVfbmFtZSI6IlJEQ19TVEFGRiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NjA0MTksImV4cCI6MTc3MDg2NzYxOSwiaWF0IjoxNzcwODYwNDE5LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.2oTGsjcXwWn8DOTIjx8mfi2EKXR3avHIokDQfeZmrJM','2026-02-12 03:40:20.028753','2026-02-12 01:40:20.028828',1),(22,47,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiNDciLCJlbWFpbCI6ImthbWFuaUBnbWFpbC5jb20iLCJ1bmlxdWVfbmFtZSI6IkthbWFuaSBQZXJlcmEiLCJyb2xlIjoiQ1VTVE9NRVIiLCJyb2xlX25hbWUiOiJDVVNUT01FUiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NjA4OTksImV4cCI6MTc3MDg2ODA5OSwiaWF0IjoxNzcwODYwODk5LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.DUCOgj6WxQhQdV5ckLj5vTz2krME5YewUQdg8FQHDMQ','2026-02-12 03:48:19.504328','2026-02-12 01:48:19.504384',1),(23,24,'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX2lkIjoiMjQiLCJlbWFpbCI6Indlc3QtcmRjQGlzZG4ubGsiLCJ1bmlxdWVfbmFtZSI6Ildlc3QgUkRDIFN0YWZmIiwicm9sZSI6IlJEQ19TVEFGRiIsInJvbGVfbmFtZSI6IlJEQ19TVEFGRiIsInJkY19pZCI6IjQiLCJuYmYiOjE3NzA4NjI2ODgsImV4cCI6MTc3MDg2OTg4OCwiaWF0IjoxNzcwODYyNjg4LCJpc3MiOiJJU0RORGlzdHJpYnV0aW9uU3lzdGVtIiwiYXVkIjoiSVNETlVzZXJzIn0.paBYIqoDsQFtkz9OQdp9BECe4mk1FXpp3pOAGWx_j2c','2026-02-12 04:18:08.826037','2026-02-12 02:18:08.826038',0);
/*!40000 ALTER TABLE `jwt_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_items`
--

DROP TABLE IF EXISTS `order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_items` (
  `order_item_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) NOT NULL,
  PRIMARY KEY (`order_item_id`),
  KEY `IX_order_items_order_id` (`order_id`),
  KEY `IX_order_items_product_id` (`product_id`),
  CONSTRAINT `FK_order_items_orders_order_id` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE,
  CONSTRAINT `FK_order_items_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_items`
--

LOCK TABLES `order_items` WRITE;
/*!40000 ALTER TABLE `order_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `order_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_returns`
--

DROP TABLE IF EXISTS `order_returns`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_returns` (
  `return_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int DEFAULT NULL,
  `product_id` int DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  `reason` text,
  `refund_status` varchar(50) DEFAULT 'Pending',
  `processed_by_id` int DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `return_type` varchar(50) DEFAULT NULL,
  `admin_comment` text,
  `reason_id` int DEFAULT NULL,
  `other_reason_description` text,
  PRIMARY KEY (`return_id`),
  KEY `order_id` (`order_id`),
  KEY `processed_by_id` (`processed_by_id`),
  KEY `reason_id` (`reason_id`),
  CONSTRAINT `order_returns_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`),
  CONSTRAINT `order_returns_ibfk_2` FOREIGN KEY (`processed_by_id`) REFERENCES `users` (`user_id`),
  CONSTRAINT `order_returns_ibfk_3` FOREIGN KEY (`reason_id`) REFERENCES `return_reasons` (`reason_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_returns`
--

LOCK TABLES `order_returns` WRITE;
/*!40000 ALTER TABLE `order_returns` DISABLE KEYS */;
/*!40000 ALTER TABLE `order_returns` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_status_logs`
--

DROP TABLE IF EXISTS `order_status_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_status_logs` (
  `status_log_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `status` varchar(50) NOT NULL,
  `updated_by_id` int NOT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`status_log_id`),
  KEY `order_id` (`order_id`),
  KEY `updated_by_id` (`updated_by_id`),
  CONSTRAINT `order_status_logs_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`),
  CONSTRAINT `order_status_logs_ibfk_2` FOREIGN KEY (`updated_by_id`) REFERENCES `users` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_status_logs`
--

LOCK TABLES `order_status_logs` WRITE;
/*!40000 ALTER TABLE `order_status_logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `order_status_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `order_id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `customer_id` int DEFAULT NULL,
  `rdc_id` int DEFAULT NULL,
  `order_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `order_date` datetime(6) NOT NULL,
  `total_amount` decimal(10,2) NOT NULL,
  `status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `delivery_address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `created_at` datetime(6) NOT NULL,
  `estimated_delivery_at` datetime DEFAULT NULL,
  PRIMARY KEY (`order_id`),
  UNIQUE KEY `IX_orders_order_number` (`order_number`),
  KEY `IX_orders_customer_id` (`customer_id`),
  KEY `IX_orders_rdc_id` (`rdc_id`),
  KEY `IX_orders_user_id` (`user_id`),
  CONSTRAINT `FK_orders_customers_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_orders_rdcs_rdc_id` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_orders_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `payment_id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `rdc_id` int DEFAULT NULL,
  `amount` decimal(10,2) NOT NULL,
  `payment_method` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `payment_status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `transaction_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `payment_date` datetime(6) DEFAULT NULL,
  `created_at` datetime(6) NOT NULL,
  PRIMARY KEY (`payment_id`),
  KEY `IX_payments_order_id` (`order_id`),
  KEY `IX_payments_rdc_id` (`rdc_id`),
  CONSTRAINT `FK_payments_orders_order_id` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE,
  CONSTRAINT `FK_payments_rdcs_rdc_id` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payments`
--

LOCK TABLES `payments` WRITE;
/*!40000 ALTER TABLE `payments` DISABLE KEYS */;
/*!40000 ALTER TABLE `payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `permissions`
--

DROP TABLE IF EXISTS `permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permissions` (
  `permission_id` int NOT NULL AUTO_INCREMENT,
  `permission_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`permission_id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permissions`
--

LOCK TABLES `permissions` WRITE;
/*!40000 ALTER TABLE `permissions` DISABLE KEYS */;
INSERT INTO `permissions` VALUES (1,'VIEW_USERS','View user list'),(2,'CREATE_USERS','Create new users'),(3,'EDIT_USERS','Edit existing users'),(4,'DELETE_USERS','Delete users'),(5,'VIEW_PRODUCTS','View product catalog'),(6,'MANAGE_PRODUCTS','Create/Edit/Delete products'),(7,'VIEW_ORDERS','View orders'),(8,'PROCESS_ORDERS','Process and manage orders'),(9,'VIEW_INVENTORY','View inventory levels'),(10,'MANAGE_INVENTORY','Update inventory levels'),(11,'VIEW_DELIVERIES','View delivery schedules'),(12,'MANAGE_DELIVERIES','Schedule and manage deliveries'),(13,'VIEW_PAYMENTS','View payment records'),(14,'PROCESS_PAYMENTS','Process payments'),(15,'VIEW_REPORTS','View system reports'),(16,'VIEW_AUDIT_LOGS','View audit logs');
/*!40000 ALTER TABLE `permissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `product_id` int NOT NULL AUTO_INCREMENT,
  `product_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `sku` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  `category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `product_image_url` varchar(255) DEFAULT '/images/default-product.png',
  PRIMARY KEY (`product_id`),
  UNIQUE KEY `IX_products_sku` (`sku`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'Rice - 5kg','Premium quality rice','RICE-5KG',850.00,'Staple Foods',1,'2026-02-10 14:54:36.653471','/images/default-product.png'),(2,'Sugar - 1kg','White refined sugar','SUGAR-1KG',150.00,'Condiments & Spices',1,'2026-02-10 14:54:36.653675','/images/default-product.png'),(3,'Flour - 1kg','All-purpose wheat flour','FLOUR-1KG',120.00,'Staple Foods',1,'2026-02-10 14:54:36.653676','/images/default-product.png'),(4,'Cooking Oil - 1L','Vegetable cooking oil','OIL-1L',450.00,'Household Essential',1,'2026-02-10 14:54:36.653684','/images/default-product.png'),(5,'Tea - 200g','Premium Ceylon tea','TEA-200G',380.00,'Dairy & Beverages',1,'2026-02-10 14:54:36.653685','/images/default-product.png'),(8,'Red Raw Rice - 5kg','Nutritious local red raw rice','RICE-RED-5K',1100.00,'Staple Foods',1,'2026-02-12 07:04:22.000000','/images/red-rice.jpg'),(9,'Basmati Rice - 2kg','Long grain aromatic basmati rice','RICE-BAS-2K',1450.00,'Staple Foods',1,'2026-02-12 07:04:22.000000','/images/basmati.jpg'),(10,'Full Cream Milk Powder - 400g','Rich and creamy instant milk powder','MILK-400G',1020.00,'Dairy & Beverages',1,'2026-02-12 07:04:22.000000','/images/milk-powder.jpg'),(11,'Instant Coffee - 50g','Premium roasted coffee beans blend','CONF-50G',650.00,'Dairy & Beverages',1,'2026-02-12 07:04:22.000000','/images/coffee.jpg'),(12,'Fruit Nectar - 1L','Natural mixed fruit juice drink','JUICE-1L',480.00,'Dairy & Beverages',1,'2026-02-12 07:04:22.000000','/images/juice.jpg'),(13,'Malt Drink - 400g','Energy boosting chocolate malt drink','MALT-400G',950.00,'Dairy & Beverages',1,'2026-02-12 07:04:22.000000','/images/malt.jpg'),(14,'Beauty Soap - 100g','Moisturizing soap with floral fragrance','SOAP-100G',145.00,'Personal Care',1,'2026-02-12 07:04:22.000000','/images/soap.jpg'),(15,'Herbal Toothpaste - 120g','Natural gum protection toothpaste','PASTE-120G',280.00,'Personal Care',1,'2026-02-12 07:04:22.000000','/images/toothpaste.jpg'),(16,'Anti-Dandruff Shampoo - 180ml','Scalp care shampoo with menthol','SHAM-180M',550.00,'Personal Care',1,'2026-02-12 07:04:22.000000','/images/shampoo.jpg'),(17,'Hand Wash - 200ml','Antibacterial liquid hand soap','HWASH-200',320.00,'Personal Care',1,'2026-02-12 07:04:22.000000','/images/handwash.jpg'),(18,'Body Lotion - 200ml','Deep nourishing winter body lotion','LOT-200ML',890.00,'Personal Care',1,'2026-02-12 07:04:22.000000','/images/lotion.jpg'),(19,'Dishwash Liquid - 500ml','Tough grease cutting lemon formula','DISH-500M',290.00,'Household Essential',1,'2026-02-12 07:04:22.000000','/images/dishwash.jpg'),(20,'Laundry Detergent - 1kg','Top load fabric cleaning powder','DETER-1KG',680.00,'Household Essential',1,'2026-02-12 07:04:22.000000','/images/detergent.jpg'),(21,'Floor Cleaner - 500ml','Germ-kill surface disinfectant','FLOOR-500',450.00,'Household Essential',1,'2026-02-12 07:04:22.000000','/images/floorclean.jpg'),(22,'Toilet Cleaner - 750ml','Extra strong stain remover','TOIL-750M',520.00,'Household Essential',1,'2026-02-12 07:04:22.000000','/images/toiletclean.jpg'),(23,'Air Freshener - 300ml','Long lasting jasmine scent spray','AIR-300ML',750.00,'Household Essential',1,'2026-02-12 07:04:22.000000','/images/airfresh.jpg'),(24,'Chocolate Cream Biscuits - 200g','Crunchy biscuits with cocoa filling','BISC-CHO',220.00,'Snacks & Confectionery',1,'2026-02-12 07:04:22.000000','/images/biscuits.jpg'),(25,'Milk Chocolate Bar - 50g','Smooth melt-in-mouth milk chocolate','CHOC-50G',180.00,'Snacks & Confectionery',1,'2026-02-12 07:04:22.000000','/images/chocolate.jpg'),(26,'Potato Chips - 100g','Salted crispy potato wafers','CHIPS-100',350.00,'Snacks & Confectionery',1,'2026-02-12 07:04:22.000000','/images/chips.jpg'),(27,'Roasted Cashews - 50g','Salted premium quality cashew nuts','CASH-50G',450.00,'Snacks & Confectionery',1,'2026-02-12 07:04:22.000000','/images/cashew.jpg'),(28,'Assorted Toffees - 150g','Mix of fruit and caramel candies','TOF-150G',250.00,'Snacks & Confectionery',1,'2026-02-12 07:04:22.000000','/images/toffees.jpg'),(29,'Chili Powder - 100g','Ground sun-dried red chilies','SPICE-CHIL',380.00,'Condiments & Spices',1,'2026-02-12 07:04:22.000000','/images/chili.jpg'),(30,'Table Salt - 1kg','Iodized fine grain table salt','SALT-1KG',120.00,'Condiments & Spices',1,'2026-02-12 07:04:22.000000','/images/salt.jpg'),(31,'Tomato Ketchup - 400g','Tangy tomato sauce with spices','SAUCE-TOM',420.00,'Condiments & Spices',1,'2026-02-12 07:04:22.000000','/images/ketchup.jpg'),(32,'Soya Sauce - 200ml','Traditional brewed savory soya sauce','SOYA-200M',310.00,'Condiments & Spices',1,'2026-02-12 07:04:22.000000','/images/soyasauce.jpg');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rdcs`
--

DROP TABLE IF EXISTS `rdcs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rdcs` (
  `rdc_id` int NOT NULL AUTO_INCREMENT,
  `rdc_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `rdc_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `region` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `contact_number` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  PRIMARY KEY (`rdc_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rdcs`
--

LOCK TABLES `rdcs` WRITE;
/*!40000 ALTER TABLE `rdcs` DISABLE KEYS */;
INSERT INTO `rdcs` VALUES (1,'North RDC','NORTH','Northern','Jaffna','+94771234501',1),(2,'South RDC','SOUTH','Southern','Galle','+94771234502',1),(3,'East RDC','EAST','Eastern','Batticaloa','+94771234503',1),(4,'West RDC','WEST','Western','Colombo','+94771234504',1),(5,'Central RDC','CENTRAL','Central','Kandy','+94771234505',1);
/*!40000 ALTER TABLE `rdcs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `return_reasons`
--

DROP TABLE IF EXISTS `return_reasons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `return_reasons` (
  `reason_id` int NOT NULL AUTO_INCREMENT,
  `reason_text` varchar(100) NOT NULL,
  PRIMARY KEY (`reason_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `return_reasons`
--

LOCK TABLES `return_reasons` WRITE;
/*!40000 ALTER TABLE `return_reasons` DISABLE KEYS */;
INSERT INTO `return_reasons` VALUES (1,'Damaged Goods'),(2,'Expired Goods'),(3,'Wrong Item Received'),(4,'Other');
/*!40000 ALTER TABLE `return_reasons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role_permissions`
--

DROP TABLE IF EXISTS `role_permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_permissions` (
  `role_permission_id` int NOT NULL AUTO_INCREMENT,
  `role_id` int NOT NULL,
  `permission_id` int NOT NULL,
  PRIMARY KEY (`role_permission_id`),
  KEY `IX_role_permissions_permission_id` (`permission_id`),
  KEY `IX_role_permissions_role_id` (`role_id`),
  CONSTRAINT `FK_role_permissions_permissions_permission_id` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`permission_id`) ON DELETE CASCADE,
  CONSTRAINT `FK_role_permissions_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role_permissions`
--

LOCK TABLES `role_permissions` WRITE;
/*!40000 ALTER TABLE `role_permissions` DISABLE KEYS */;
/*!40000 ALTER TABLE `role_permissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `role_id` int NOT NULL AUTO_INCREMENT,
  `role_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `parent_role_id` int DEFAULT NULL,
  PRIMARY KEY (`role_id`),
  UNIQUE KEY `IX_roles_role_name` (`role_name`),
  KEY `IX_roles_parent_role_id` (`parent_role_id`),
  CONSTRAINT `FK_roles_roles_parent_role_id` FOREIGN KEY (`parent_role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'ADMIN',NULL),(2,'HEAD_OFFICE',NULL),(3,'RDC_STAFF',NULL),(4,'LOGISTICS',NULL),(5,'DRIVER',NULL),(6,'FINANCE',NULL),(7,'SALES_REP',NULL),(8,'CUSTOMER',NULL);
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `stock_transfers`
--

DROP TABLE IF EXISTS `stock_transfers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stock_transfers` (
  `transfer_id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `from_rdc_id` int NOT NULL,
  `to_rdc_id` int NOT NULL,
  `quantity` int NOT NULL,
  `status` enum('PENDING','IN_TRANSIT','COMPLETED','CANCELLED') DEFAULT 'PENDING',
  `requested_by` int NOT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`transfer_id`),
  KEY `product_id` (`product_id`),
  KEY `from_rdc_id` (`from_rdc_id`),
  KEY `to_rdc_id` (`to_rdc_id`),
  CONSTRAINT `stock_transfers_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`),
  CONSTRAINT `stock_transfers_ibfk_2` FOREIGN KEY (`from_rdc_id`) REFERENCES `rdcs` (`rdc_id`),
  CONSTRAINT `stock_transfers_ibfk_3` FOREIGN KEY (`to_rdc_id`) REFERENCES `rdcs` (`rdc_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock_transfers`
--

LOCK TABLES `stock_transfers` WRITE;
/*!40000 ALTER TABLE `stock_transfers` DISABLE KEYS */;
/*!40000 ALTER TABLE `stock_transfers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `user_id` int NOT NULL AUTO_INCREMENT,
  `full_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `password_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `role_id` int NOT NULL,
  `rdc_id` int DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL,
  `two_factor_enabled` tinyint(1) NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `last_login` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `IX_users_email` (`email`),
  KEY `IX_users_rdc_id` (`rdc_id`),
  KEY `IX_users_role_id` (`role_id`),
  CONSTRAINT `FK_users_rdcs_rdc_id` FOREIGN KEY (`rdc_id`) REFERENCES `rdcs` (`rdc_id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_users_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'System Administrator','admin@isdn.lk','$2a$11$o1d.uM0.QfH0J4nW9PXO3uMPk./uQ2XTZwuwcd1bojMhNVSDhpuYK',1,NULL,1,0,'2026-02-10 14:54:30.983696','2026-02-11 10:38:55.909033'),(2,'Head Office Manager','headoffice@isdn.lk','$2a$11$O7beh5FOt91MuiPJS9ryLO14ObB0msZlp3UQt.BjfR2mI7Gk8pjhK',2,NULL,1,0,'2026-02-10 14:54:31.709849','2026-02-11 19:46:42.044896'),(3,'RDC Staff Member (Unassigned)','rdc@isdn.lk','$2a$11$Pwcsu4iXDiassqlcGoO6weS/9wzlRDgu7FCIm0E4hllt54Lqw2miu',3,NULL,1,0,'2026-02-10 14:54:31.863566',NULL),(4,'Logistics Coordinator (Unassigned)','logistics@isdn.lk','$2a$11$ztYyf7OqJzBVrllFj3nsyuqjavdP.dhcMcuqGp2pOo6VZ3CJtnLHq',4,NULL,1,0,'2026-02-10 14:54:32.018551',NULL),(5,'Delivery Driver (Unassigned)','driver@isdn.lk','$2a$11$7jtkBi7yYQxK04H5.x0BQeXEzWXfxmAWiPPJ4rQl.wYymD4zagEOu',5,NULL,1,0,'2026-02-10 14:54:32.181968',NULL),(6,'Finance Officer (Unassigned)','finance@isdn.lk','$2a$11$TdZO.Lnz9.MNfAeL6B9WbuYSkMRkGeIBLjCvEzR55bX043/hXzCFe',6,NULL,1,0,'2026-02-10 14:54:32.349332',NULL),(7,'Sales Representative (Unassigned)','sales@isdn.lk','$2a$11$wzZxXVhAhF7.iN.dHk/7VuimDtQEfAWdj7tdkZhr3NSWwPJJDsNYu',7,NULL,1,0,'2026-02-10 14:54:32.499809',NULL),(8,'Test Customer','customer@test.com','$2a$11$wUqZQBwR8hVeDOOg8xupnubWYDclMg8/BwsHzatmbpJU/eU2cpx0i',8,NULL,1,0,'2026-02-10 14:54:32.656185',NULL),(9,'North RDC Staff','north-rdc@isdn.lk','$2a$11$IPmoZxJgRWyk9vfpjf5ySuStIa1O2Ea8YMbwAvswRfz0V6clI9SOe',3,1,1,0,'2026-02-10 14:54:32.810579','2026-02-11 10:43:20.411902'),(10,'North Logistics','north-logistics@isdn.lk','$2a$11$Y5qiy41gC2L619f1gQZyAeuPLrRRxvj7D5CCkLDcynSqoSLp8.DFy',4,1,1,0,'2026-02-10 14:54:32.960102','2026-02-11 10:44:42.696623'),(11,'North Driver','north-driver@isdn.lk','$2a$11$cgqDyvif.8UI9mEnz773aO/wIjG/75W45kuCnbsVOjkbwr4zDx906',5,1,1,0,'2026-02-10 14:54:33.109183',NULL),(12,'North Finance','north-finance@isdn.lk','$2a$11$TwQLu6O41kNcngDimmyUkuJPbLcVfW3drrtwNdrU80mOkXlh5imhq',6,1,1,0,'2026-02-10 14:54:33.274569',NULL),(13,'North Sales Rep','north-sales@isdn.lk','$2a$11$jDBazOwQr.RJhHTzbX5Rz.DKVhu8z4ghU4Z9Z0HZmsE9i5vntkVLK',7,1,1,0,'2026-02-10 14:54:33.430760',NULL),(14,'South RDC Staff','south-rdc@isdn.lk','$2a$11$/Ok/aQmSNwunK9QdpyPI1OiX4K52VLw8eNeT3zUiK43t4Xsjt2Ieu',3,2,1,0,'2026-02-10 14:54:33.584437',NULL),(15,'South Logistics','south-logistics@isdn.lk','$2a$11$/YEPKT9aLu6HTgAp/pWGge8OVW/9M3imnuKk97qklJZIb.smNOARm',4,2,1,0,'2026-02-10 14:54:33.736232','2026-02-10 16:37:20.890873'),(16,'South Driver','south-driver@isdn.lk','$2a$11$uC3M3lReNx0Xwl.uZhg1puKeI9W7VPSnChlKb6QLZdjipjXOQFb0m',5,2,1,0,'2026-02-10 14:54:33.889248',NULL),(17,'South Finance','south-finance@isdn.lk','$2a$11$oxVeJsXfdv5w3ZDRR9JbJeZRymMmV9MurgHSqbxqQl0lTms4jexCW',6,2,1,0,'2026-02-10 14:54:34.039153',NULL),(18,'South Sales Rep','south-sales@isdn.lk','$2a$11$bYjhY60iGl65fqa0qn.UeOyV1cn1cVIEwjYB8jpJE3SxtQ/faXUju',7,2,1,0,'2026-02-10 14:54:34.192691','2026-02-10 16:38:16.719742'),(19,'East RDC Staff','east-rdc@isdn.lk','$2a$11$IkYRcmkV59JU3DXZkw88g.JmGrt2XUxNlV/3MmvfUX2FmoIddYgyK',3,3,1,0,'2026-02-10 14:54:34.359053',NULL),(20,'East Logistics','east-logistics@isdn.lk','$2a$11$C85bRFAu30txfCWKh.gc5uJSF/NCkfHAFArVyhRc.Q5FaTPH7wi1K',4,3,1,0,'2026-02-10 14:54:34.516636','2026-02-10 15:01:44.496848'),(21,'East Driver','east-driver@isdn.lk','$2a$11$wHgS1reGZdWR06ghhxokDuaq0C0WIUvHoYYeeWsocOQ5wgMU6gBTW',5,3,1,0,'2026-02-10 14:54:34.672697',NULL),(22,'East Finance','east-finance@isdn.lk','$2a$11$N.zKna/bUSDdmTdaOx//7OiWYQ5wLT2F0j5Z7uUh6VQfxqaVyBIR6',6,3,1,0,'2026-02-10 14:54:34.822450',NULL),(23,'East Sales Rep','east-sales@isdn.lk','$2a$11$5Utw6FhvUdcdASohw.0Qme2NXuwxCj8TT3pMLuLLn1So05D.FqRR2',7,3,1,0,'2026-02-10 14:54:34.978932',NULL),(24,'West RDC Staff','west-rdc@isdn.lk','$2a$11$.IzS6G2NwWUzKlLgZBv9pONVaAiqGPFx7ysNqDj.0EffdQiKZHuBC',3,4,1,0,'2026-02-10 14:54:35.127393','2026-02-12 02:18:08.837797'),(25,'West Logistics','west-logistics@isdn.lk','$2a$11$M7q6OaXqErkCgOPpnvDPle7RI6z7Jp2tiR/.2OKWA3vAi.Cx/oaba',4,4,1,0,'2026-02-10 14:54:35.282648',NULL),(26,'West Driver','west-driver@isdn.lk','$2a$11$chuYBbIuUIT7Mre.5P0/8.72yrk2afCgRA97CJ7.iaO1Y6Raik1bi',5,4,1,0,'2026-02-10 14:54:35.455128',NULL),(27,'West Finance','west-finance@isdn.lk','$2a$11$vjyfiye8mWGo6gxSvk/bOOw/CEJbPnDtkxAUp9dDvGVavWX2fLyn.',6,4,1,0,'2026-02-10 14:54:35.610806',NULL),(28,'West Sales Rep','west-sales@isdn.lk','$2a$11$hckNhdJ.p0FSWAO.ezwbhuYzADX846G.wtZEVYl5UIbuap.BRoT82',7,4,1,0,'2026-02-10 14:54:35.767520',NULL),(29,'Central RDC Staff','central-rdc@isdn.lk','$2a$11$isAPRsyP7e0i5Z92BLLDIeDZKIvcXPkRuhWnNamUZvEdutbP75PMa',3,5,1,0,'2026-02-10 14:54:35.921584','2026-02-10 15:00:35.573709'),(30,'Central Logistics','central-logistics@isdn.lk','$2a$11$wdLv2f6m4Lro4Hsj7wisZeXp46WLhLfkKTpF3f2R7DRbFR8oLPorK',4,5,1,0,'2026-02-10 14:54:36.077603',NULL),(31,'Central Driver','central-driver@isdn.lk','$2a$11$e0VH9rEu/l5ex2FTNbKu8OE3f5R.QXZFv3jxLot5NBRKhq9.NZ/Wi',5,5,1,0,'2026-02-10 14:54:36.230659',NULL),(32,'Central Finance','central-finance@isdn.lk','$2a$11$XaR9.ZX564RjoOfyuW9d4uK0OeCXa/8jDA7XvtFafRWGIm96Vkqp.',6,5,1,0,'2026-02-10 14:54:36.380720',NULL),(33,'Central Sales Rep','central-sales@isdn.lk','$2a$11$uBQcsepRKZXGshbK8e9GUu/BmrWQ7MEIyh.EZ8npS3aBgjcfmcyYu',7,5,1,0,'2026-02-10 14:54:36.549795',NULL),(47,'Kamani Perera','kamani@gmail.com','$2a$11$vwbjF6tuMu3DUuDYGoAhcuTkuRAn3aDYB0WH4wLa5TEFKVF2PmvEy',8,4,1,0,'2026-02-11 18:14:03.084767','2026-02-12 01:48:19.518511'),(50,'Sudath Perera','sudath@gmail.com','$2a$11$SpFxJwRHIOpEV0o.GaEsXOlQR92K9mbSYUKCEu8S3j.C3RtQ7WU7a',8,4,1,0,'2026-02-11 18:17:22.010457',NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-12 12:27:00
