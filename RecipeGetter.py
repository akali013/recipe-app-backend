#!/usr/bin/env python
# coding: utf-8

# Python API source: https://www.geeksforgeeks.org/python/how-to-make-api-calls-using-python/
# Use the recipes API to get all meals starting with every letter of the English alphabet
import requests
import pandas as pd

def get_all_recipes():
    baseURL = "https://www.themealdb.com/api/json/v1/1/search.php?f="
    recipeData = {"recipes": []}   # Each recipe is represented by a dict
    for letter in "abcdefghijklmnopqrstuvwxyz":
        try:
            response = requests.get(baseURL + letter)
            
            if response.status_code == 200:
                recipes = response.json()

                # If the current letter has recipes, add it to the overall data
                if recipes["meals"] is not None:
                    revisedRecipes = [convertRecipe(recipe) for recipe in recipes["meals"]]
                    recipeData["recipes"].extend(revisedRecipes)
            else:
                print("Error:", response.status_code)
                return None
        except requests.exceptions.RequestException as e:
            print("Error:", e)
            return None
    return recipeData

# Reformat the data to match the Recipe Model in the .NET app
def convertRecipe(recipe):
    return {
        "Id": recipe["idMeal"],
        "Name": recipe["strMeal"],
        "Type": recipe["strCategory"],
        "Ingredients": getIngredients(recipe),
        "Instructions": recipe["strInstructions"].replace("\r", "").replace("\n", "").split("."),
        "Source": recipe["strSource"],
        "ImageUrl": recipe["strMealThumb"]
    }

# Combine the ingredients and measurements from the API into a single list
def getIngredients(recipe):
    ingredients = ""
    info = ""
    i = 1
    currentMeasure = str(recipe["strMeasure1"])
    currentIngredient = str(recipe["strIngredient1"])
    
    while (currentMeasure != "" and currentIngredient != "" and currentMeasure != "None" and currentIngredient != "None" and i < 20):
        info = currentMeasure + " " + currentIngredient
        info = info.replace("\\", " ")
        ingredients += info + "\\"      # Separate ingredients by a backslash so they can be separated later into a list.
        i += 1
        currentIngredient = str(recipe["strIngredient" + str(i)])
        currentMeasure = str(recipe["strMeasure" + str(i)])
        
    return ingredients.split("\\")
    

# Convert the JSON data to a JSON file so the .NET app can seed the db
recipeJSON = get_all_recipes()

df = pd.json_normalize(recipeJSON["recipes"])
df.to_json("RecipeData.json", orient="records")