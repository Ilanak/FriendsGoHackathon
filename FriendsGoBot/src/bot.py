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

def error(bot, update, error):
    logger.warn('Update "%s" caused error "%s"' % (update, error))

def handleJoinCommand(bot, update):
    join = {"Name": update.message.from_user.name, "Id": update.message.from_user.id}
    url = base_url+ str(abs(update.message.chat_id))+'/join'
    resp = requests.post(url, json=join)
    if resp.status_code == 200:
        bot.send_message(update.message.chat_id, str(resp.content))
    else:
        bot.send_message(update.message.chat_id, "Error")


def handleStartGameCommand(bot, update):
    checkin = {}
    url = base_url + str(abs(update.message.chat_id)) + '/go/'+ str(abs(update.message.from_user.id))
    resp = requests.post(url, json=checkin)

    button = telegram.KeyboardButton("Share location", request_location=True)
    customKeyboard = [[button]]
    reply_markup = telegram.ReplyKeyboardMarkup(keyboard=customKeyboard, resize_keyboard=True, one_time_keyboard=True)
    bot.send_message(update.message.from_user.id, "Share your location", reply_markup=reply_markup)

def handleChallengeCommand(bot, update):
    url = base_url + str(abs(update.message.chat_id)) + '/mission'
    resp = requests.get(url)
    if resp.status_code == 200:
        bot.send_message(update.message.chat_id, str(resp.content))
    else:
        bot.send_message(update.message.chat_id, "Error")

def handleCheckinCommand(bot, update):
    checkin = {}
    url = base_url + str(abs(update.message.chat_id)) + '/checkin/' + str(abs(update.message.from_user.id))
    resp = requests.post(url, json=checkin)

    button = telegram.KeyboardButton("Share location", request_location=True)
    customKeyboard = [[button]]
    reply_markup = telegram.ReplyKeyboardMarkup(keyboard=customKeyboard, resize_keyboard=True, one_time_keyboard=True)
    bot.send_message(update.message.from_user.id, "Share your location", reply_markup=reply_markup)

def handleStatCommand(bot, update):
    bot.send_message(update.message.chat_id, "Group status: you are on level XXX, Good job!")
    #url = base_url + str(abs(update.message.chat_id)) + '/Stat'
    #resp = requests.get(url)
    #if resp.status_code == 200:
    #   bot.send_message(update.message.chat_id, str(resp.content))

#task = {"summary": "Take out trash", "description": "" }
#resp = requests.post('https://todolist.example.com/tasks/', json=task)

#change to py string str(update.message.chat_id)


def handleMyLocationCommand(bot, update):
    user = str(abs(update.message.from_user.id))
    latitude = str(update.message.location.latitude)
    longitude = str(update.message.location.longitude)
    location = {"UserId": user ,"Latitude": latitude, "Longitude": longitude}
    url = base_url + '/location'
    resp = requests.post(url, json=location)

    if resp.status_code == 200:
        bot.send_message(update.message.from_user.id, resp.content)

        #bot.send_message(str(resp.content)
        #jsonResp = resp.content
        #bot.send_message(jsonResp["GroupId"], jsonResp["Message"])
    else:
        bot.send_message(update.message.chat_id, "Error")

def main():
    updater = Updater(token="269182723:AAG26YGMIwHaSi6oGQUdM2kABsdeMVNKSdA")
    dp = updater.dispatcher
    dp.add_handler(CommandHandler("join", handleJoinCommand))
    dp.add_handler(CommandHandler("mission", handleChallengeCommand))
    dp.add_handler(CommandHandler("go", handleStartGameCommand))
    dp.add_handler(CommandHandler("stat", handleStatCommand))
    dp.add_handler(CommandHandler("checkin", handleCheckinCommand))

    dp.add_handler(MessageHandler([Filters.location], handleMyLocationCommand))
    dp.add_error_handler(error)

    updater.start_polling()

    # Run the bot until the you presses Ctrl-C or the process receives SIGINT,
    # SIGTERM or SIGABRT. This should be used most of the time, since
    # start_polling() is non-blocking and will stop the bot gracefully.
    updater.idle()

main()



