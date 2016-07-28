import telegram
from telegram.ext import Updater, CommandHandler, MessageHandler, Filters
import logging
import requests

# Enable logging
logging.basicConfig(
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    level=logging.INFO)

logger = logging.getLogger(__name__)

base_url = 'https://friendsgowebapi.azurewebsites.net/api/Game/'
#base_url = 'http://localhost:57466/api/Game/'

def error(bot, update, error):
    logger.warn('Update "%s" caused error "%s"' % (update, error))

def handleJoinCommand(bot, update):
    join = {"Name": update.message.from_user.name, "Id": update.message.from_user.id}
    url = base_url+ str((update.message.chat_id))+'/join'
    resp = requests.post(url, json=join)
    if resp.status_code != 200:
        bot.send_message(update.message.chat_id, "Status not 200")


def handleStartGameCommand(bot, update):
    checkin = {}
    url = base_url + str((update.message.chat_id)) + '/go/'+ str((update.message.from_user.id))
    resp = requests.post(url, json=checkin)

    button = telegram.KeyboardButton("Share location", request_location=True)
    customKeyboard = [[button]]
    reply_markup = telegram.ReplyKeyboardMarkup(keyboard=customKeyboard, resize_keyboard=True, one_time_keyboard=True)
    bot.send_message(update.message.from_user.id, "Share your location", reply_markup=reply_markup)

def handleChallengeCommand(bot, update):
    url = base_url + str((update.message.chat_id)) + '/mission'
    resp = requests.get(url)
    if resp.status_code != 200:
        bot.send_message(update.message.chat_id, "Status not 200")

def handleCheckinCommand(bot, update):
    checkin = {}
    url = base_url + str((update.message.chat_id)) + '/checkin/' + str((update.message.from_user.id))
    resp = requests.post(url, json=checkin)

    button = telegram.KeyboardButton("Share location", request_location=True)
    customKeyboard = [[button]]
    reply_markup = telegram.ReplyKeyboardMarkup(keyboard=customKeyboard, resize_keyboard=True, one_time_keyboard=True)
    bot.send_message(update.message.from_user.id, "Share your location", reply_markup=reply_markup)

def handleStatCommand(bot, update):
    url = base_url + str(update.message.chat_id) + '/stat'
    resp = requests.get(url)
    if resp.status_code != 200:
        bot.send_message(update.message.chat_id, "Status not 200")

def handleGlobStatCommand(bot, update):
    url = base_url + str(update.message.chat_id) + '/globalStat'
    resp = requests.get(url)
    #if resp.status_code != 200:
    #    bot.send_message(update.message.chat_id, "Error")
    if resp.status_code != 200:
        bot.send_message(update.message.chat_id, "Status not 200")


def handleMyLocationCommand(bot, update):
    user = str((update.message.from_user.id))
    latitude = str(update.message.location.latitude)
    longitude = str(update.message.location.longitude)
    location = {"UserId": user ,"Latitude": latitude, "Longitude": longitude}
    url = base_url + '/location'
    resp = requests.post(url, json=location)

    if resp.status_code != 200:
        bot.send_message(update.message.chat_id, "Status not 200")

def main():
    updater = Updater(token="269182723:AAFP1qBAcnfnY0g9HHkw0a4jR69DmroR4Gg")
    dp = updater.dispatcher
    dp.add_handler(CommandHandler("join", handleJoinCommand))
    dp.add_handler(CommandHandler("mission", handleChallengeCommand))
    dp.add_handler(CommandHandler("go", handleStartGameCommand))
    dp.add_handler(CommandHandler("stat", handleStatCommand))
    dp.add_handler(CommandHandler("globalstat", handleGlobStatCommand))
    dp.add_handler(CommandHandler("checkin", handleCheckinCommand))

    dp.add_handler(MessageHandler([Filters.location], handleMyLocationCommand))
    dp.add_error_handler(error)

    updater.start_polling()

    # Run the bot until the you presses Ctrl-C or the process receives SIGINT,
    # SIGTERM or SIGABRT. This should be used most of the time, since
    # start_polling() is non-blocking and will stop the bot gracefully.
    updater.idle()

main()



