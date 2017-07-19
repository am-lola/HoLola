#pragma once

#ifndef _SOCK_UTILS_H
#define _SOCK_UTILS_H

#include <iostream>
#include <string>
#include <codecvt>

#include <WinSock2.h>
#include <WS2tcpip.h>

typedef SSIZE_T ssize_t;

/////////////////////////////////////////////////
///
/// A small collection of helper functions
/// for dealing with socket connections
///
/////////////////////////////////////////////////

// prints an error from WSA
inline void LogWSAErrorStr(std::wstring error)
{
    LPWSTR err = NULL;

    DWORD len = FormatMessageW(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        WSAGetLastError(),
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPWSTR)&err, 0, NULL);

    OutputDebugString((L"\tError: " + error + L" -- " + std::wstring(err)).c_str());

    LocalFree(err);
}

inline void LogWSAErrorStr(std::string error)
{
    LogWSAErrorStr(std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>>().from_bytes(error));
}

inline std::wstring GetWSAErrorStr(int error, std::wstring msg)
{
    std::wstring res;
    LPWSTR err = NULL;

    DWORD len = FormatMessageW(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        error,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPWSTR)&err, 0, NULL);
    
    return msg + L" -- " + std::to_wstring(errno) + L": " + err;
}

// gets '<hostname>:<port>' from an address
inline std::wstring hostString(const sockaddr_in& s)
{
    char ipstr[INET6_ADDRSTRLEN];
    return (std::wstring)(InetNtop(AF_INET, &(s.sin_addr), (PWSTR)ipstr, sizeof(ipstr))) + L":" + std::to_wstring(ntohs(s.sin_port));
}

// creates a new TCP socket on the given port
// which can be used to listen for new connections
static int create_server_socket(unsigned int port)
{
    struct sockaddr_in si_me;
    int s;

    // create & bind socket
    if ((s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP)) == INVALID_SOCKET)
    {
        LogWSAErrorStr(L"Failed to create socket!");
        return s;
    }

    si_me.sin_family = AF_INET;
    si_me.sin_port = htons(port);
    si_me.sin_addr.s_addr = htonl(INADDR_ANY);
    if (bind(s, (sockaddr*)&si_me, sizeof(si_me)) == -1)
    {
        LogWSAErrorStr(L"bind failed!");
        return INVALID_SOCKET;
    }

    if (listen(s, 5) != 0)
    {
    LogWSAErrorStr(L"listen failed!");
    return INVALID_SOCKET;
    }

    return s;
}

// Creates a new TCP socket connected to the given host:port
static int create_client_socket(unsigned int port, std::wstring host)
{
    ADDRINFOW hints, *res;
    int s;

    // get server info for connection
    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    int n = GetAddrInfoW(host.c_str(), std::to_wstring(port).c_str(), &hints, &res);
    if (n != 0)
    {
        LogWSAErrorStr(std::wstring(L"Could not get host info for: ") + host);
        return INVALID_SOCKET;
    }

    std::wcout << "Attempting to connect to: " << host << ":" << port << std::endl;

    // connect to server
    bool connection_success = false;
    for (auto rp = res; rp != NULL; rp = rp->ai_next) // check all addresses found by getaddrinfo
    {
        s = socket(rp->ai_family, rp->ai_socktype, rp->ai_protocol);
        if (s < 0)
            continue; // could not open socket

                      // attempt to connect
        if (connect(s, rp->ai_addr, rp->ai_addrlen) == 0)
        {
            std::wcout << "Successfully connected to " << host << ":" << port << std::endl;
            connection_success = true;
            break;
        }
        else
        {
            LogWSAErrorStr(std::wstring(L"Could not connect to ") + host);
            break;
        }

        closesocket(s); // if connection failed, close socket and move on to the next address
    }

    if (!connection_success)
    {
        std::wcout << "Connection to " << host << ":" << port << " failed! Exiting..." << std::endl;
        s = INVALID_SOCKET;
    }
    FreeAddrInfoW(res);

    return s;
}

// Creates a new UDP socket on the given port
static socklen_t create_udp_socket(unsigned int port)
{
    struct sockaddr_in si_me;
    socklen_t s;
    char   broadcast  = 1;
    char   reuseport  = 1;
    u_long nonblocking = 1;

    // create & bind socket
    if ((s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == INVALID_SOCKET)
    {
        LogWSAErrorStr(L"Creating UDP socket failed!");
        return s;
    }

    if (setsockopt(s, SOL_SOCKET, SO_BROADCAST, &broadcast, sizeof(broadcast)) != 0)
    {
        LogWSAErrorStr(L"Setting broadcast flag on UDP socket failed!");
        return INVALID_SOCKET;
    }

    if (setsockopt(s, SOL_SOCKET, SO_REUSEADDR, &reuseport, sizeof(reuseport)) != 0)
    {
        LogWSAErrorStr(L"Setting Reuse Addr flag on UDP socket failed!");
        return INVALID_SOCKET;
    }

    if (ioctlsocket(s, FIONBIO, &nonblocking) == SOCKET_ERROR)
    {
        LogWSAErrorStr(L"Setting Non-blocking flag on UDP socket failed!");
        return INVALID_SOCKET;
    }

    si_me.sin_family = AF_INET;
    si_me.sin_port = htons(port);
    si_me.sin_addr.s_addr = htonl(INADDR_ANY);
    if (bind(s, (sockaddr*)&si_me, sizeof(si_me)) != 0)
    {
        LogWSAErrorStr(L"Binding UDP socket failed!");
        return INVALID_SOCKET;
    }

    return s;
}

#endif // SOCK_UTILS_H
