# <span style="color: #2e7d32;">EcoDash</span>

**EcoDash** is a web application designed to promote eco-friendly commuting by providing real-time route suggestions from multiple transport services. It calculates CO2 savings, calories burned, and allows users to compare their progress on a leaderboard. The app aims to inspire sustainability and help users reduce their carbon footprint by making informed, greener commuting decisions.

---

## 🌟 Features

- 🚶 **Real-Time Eco-Friendly Route Suggestions**: Get route suggestions based on walking, biking, and public transportation to reduce your carbon footprint.
- 🌍 **Track CO2 Savings & Calories Burned**: Calculate and track CO2 savings and calories burned for each commute.
- 🏆 **Gamified Leaderboard**: Compare your eco-friendly progress with other users and compete to be the greenest commuter.
- 📸 **Route Image Uploads**: Upload and share images of completed routes to document your eco-friendly journeys.
- 🗺️ **Google Maps Integration**: Plan routes with accuracy using Google Maps integration for real-time suggestions and navigation.

---

## 🛠️ <span style="color: #2e7d32;">How to Run the Project</span>

### Prerequisites

Before running the project, ensure you have the following tools installed:

- **.NET Core SDK** – [Download here](https://dotnet.microsoft.com/download)
- **MongoDB** (local installation or use **MongoDB Atlas** for cloud-based storage) – [Download MongoDB](https://www.mongodb.com/try/download/community)
- **Google Maps API Key** – [Get a Google Maps API Key](https://developers.google.com/maps/documentation/javascript/get-api-key)

### Setup

Follow these steps to set up and run the project locally:

1. **Clone the Repository**  
   - Start by cloning the project repository from GitHub:
   - bash
   - git clone https://github.com/RoninDev0/EcoDash
   - cd ecodash

2. **Create a .env file in the root of the project directory and set the following variables**

- ConnectionString=<Your_MongoDB_Connection_String>
- DatabaseName=<Your_Database_Name>
- GOOGLE_MAPS_KEY=<Your_Google_Maps_API_Key>

3. **Restore Dependencies**
- dotnet restore

4. **Run Application**
- dotnet run

# Mongo DB Setup
Create 2 Collections

- Routes
- Members

## 💬 <span style="color: #2e7d32;">Contact</span>

For questions or support

- GitHub: [RoninDev0](https://github.com/RoninDev0)
- Email: ronindevhackathon@gmail.com

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.


